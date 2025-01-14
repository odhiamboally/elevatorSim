
using AutoMapper;

using ES.Application.Abstractions.ICommands;
using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Entities;
using ES.Domain.Enums;
using ES.Infrastructure.Implementations.Commands;
using ES.Infrastructure.Implementations.Interfaces;
using ES.Shared.Exceptions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using System.Runtime.InteropServices;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ES.Infrastructure.Implementations.Services;
internal sealed class ElevatorService : IElevatorService
{
    private readonly IConfiguration _config;
    private readonly IFloorService _floorService;
    private readonly IElevatorStateManager _elevatorStateManager;
    private readonly IFloorQueueManager _floorQueueManager;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ElevatorService(
        IConfiguration config, 
        IFloorService floorService, 
        IElevatorStateManager elevatorStateManager, 
        IFloorQueueManager floorQueueManager,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _config = config;
        _floorService = floorService;
        _elevatorStateManager = elevatorStateManager;
        _floorQueueManager = floorQueueManager;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        
    }



    /// <summary>
    /// Checks and processes the queues for all floors, ensuring each request is managed.
    /// </summary>
    public async Task CheckAndProcessPassengerQueues()
    {
        try
        {
            await _floorQueueManager.ProcessAllFloorQueues();
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<bool>> CompleteRequest(CompleteRequest request)
    {
        try
        {
            try
            {
                if (request.ElevatorInfo == null || request.ElevatorRequest == null)
                    return Response<bool>.Failure("Elevator or Request not found.");

                request.ElevatorInfo.DequeueRequest(request.ElevatorRequest);

                await _elevatorStateManager.FetchElevatorStateAsync(request.ElevatorInfo.Id, request.ElevatorInfo);

                var elevator = _mapper.Map<Elevator>(request.ElevatorInfo);
                await _unitOfWork.ElevatorRepository.UpdateAsync(elevator);

                return Response<bool>.Success("Request completed successfully.", true);
            }
            catch (Exception)
            {

                throw;
            }
        }
        catch (Exception)
        {

            throw;
        }
    }

    /// <summary>
    /// Dispatches the elevator to move to its next destination.
    /// </summary>
    public async Task<Response<bool>> DispatchElevator(int elevatorId, ElevatorRequest request)
    {
        try
        {
            var elevatorForDispatch = await _unitOfWork.ElevatorRepository.FindByIdAsync(elevatorId);
            if (elevatorForDispatch == null)
                return Response<bool>.Failure("Elevator not found.");

            ElevatorInfo optimalElevator = new (
                elevatorForDispatch.Id,
                elevatorForDispatch.CurrentLoad,
                elevatorForDispatch.CurrentFloor,
                elevatorForDispatch.Status,
                elevatorForDispatch.Direction

            );

            // Update the elevator state directly using encapsulated methods
            optimalElevator.UpdateStatus(ElevatorStatus.Moving);
            optimalElevator.UpdateDirection(optimalElevator.CurrentFloor < request.ToFloor ? ElevatorDirection.Up : ElevatorDirection.Down);
            optimalElevator.EnqueueRequest(request);

            await _elevatorStateManager.FetchElevatorStateAsync(elevatorId, optimalElevator);

            return Response<bool>.Success("Elevator dispatched successfully.", true);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Response<bool>> DispatchElevator(DispatchElevatorRequest request)
    {
        try
        {
            if (request.ElevatorInfo == null)
                return Response<bool>.Failure("Elevator not found.");

            // Update the elevator state directly using encapsulated methods
            request.ElevatorInfo.UpdateStatus(ElevatorStatus.Moving);
            request.ElevatorInfo.UpdateDirection(request.ElevatorInfo.CurrentFloor < request.ElevatorRequest!.ToFloor ? ElevatorDirection.Up : ElevatorDirection.Down);
            request.ElevatorInfo.EnqueueRequest(request.ElevatorRequest);

            await _elevatorStateManager.FetchElevatorStateAsync(request.ElevatorInfo.Id, request.ElevatorInfo);

            var elevator = new Elevator
            {
                Id = request.ElevatorInfo.Id,
                Capacity = request.ElevatorInfo.Capacity,
                CurrentFloor = request.ElevatorInfo.CurrentFloor,
                CurrentLoad = request.ElevatorInfo.CurrentLoad,
                Status = request.ElevatorInfo.Status,
                Direction = request.ElevatorInfo.Direction,
                RequestQueue = (Queue<int>)request.ElevatorInfo.RequestQueue.Select(x => x.Id)

            };

            //var elevator = _mapper.Map<Elevator>(request.ElevatorInfo);
            await _unitOfWork.ElevatorRepository.UpdateAsync(elevator); 

            return Response<bool>.Success("Elevator dispatched successfully.", true);
        }
        catch (Exception)
        {

            throw;
        }
    }

    /// <summary>
    /// Finds the nearest available elevator that can service a specific request.
    /// </summary>
    public async Task<Response<ElevatorInfo>> FindNearestElevator(ElevatorRequest request)
    {
        try
        {
            // Fetch all elevator states
            var elevators = _unitOfWork.ElevatorRepository.FindAll().ToList();

            if (elevators == null || !elevators.Any())
                return Response<ElevatorInfo>.Failure("No available elevator found.");

            var elevator = elevators
                .Where(e => e.Status != ElevatorStatus.OutOfService)
                .OrderBy(e => e.Status == ElevatorStatus.Idle ? 0 : 1) // Prefer idle elevators
                .ThenBy(e => e.CurrentFloor == request.FromFloor ? 0 : 1) // Prefer elevators on the same floor
                .ThenBy(e =>
                    // Prefer elevators moving in the same direction as the request
                    (e.Direction == request.Direction &&
                     ((request.Direction == ElevatorDirection.Up && e.CurrentFloor < request.ToFloor) ||
                      (request.Direction == ElevatorDirection.Down && e.CurrentFloor > request.ToFloor)))
                        ? 0 : 1)
                .ThenBy(e =>
                    // Handle edge cases: idle elevators or elevators in the opposite direction but close to the FromFloor
                    e.Direction == ElevatorDirection.Idle ||
                    (e.Direction != request.Direction && Math.Abs(e.CurrentFloor - request.FromFloor) < 3) ? 0 : 1)
                .ThenBy(e => Math.Abs(e.CurrentFloor - request.FromFloor)) // Prioritize by closest distance
                .ThenBy(e => (e.CurrentLoad / (double)e.Capacity) >= 1 ? 1 : 0) // Deprioritize full elevators
                .ThenBy(e => e.CurrentLoad / (double)e.Capacity) // Prefer elevators with more capacity available
                .FirstOrDefault();

            if (elevator == null)
                return Response<ElevatorInfo>.Failure("No available elevator found.");

            ElevatorInfo optimalElevator = new ElevatorInfo(
                elevator.Id,
                elevator.CurrentLoad,
                elevator.CurrentFloor,
                elevator.Status,
                elevator.Direction

            );

            return Response<ElevatorInfo>.Success("Nearest elevator found", optimalElevator!);
                
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Handles a partial load scenario when only part of a request can be loaded due to capacity limits.
    /// </summary>
    public async Task<Response<ElevatorInfo>> HandlePartialLoad(Elevator optimalElevator, ElevatorRequest request, int availableSpace)
    {
        try
        {
            if (availableSpace > 0)
            {
                ElevatorRequest partialRequest = new()
                {
                    Id = request.Id,
                    FromFloor = request.FromFloor,
                    ToFloor = request.ToFloor,
                    PeopleCount = Math.Min(request.PeopleCount, availableSpace),
                    Direction = request.Direction
                };

                ElevatorInfo elevatorInfo = new(
                    optimalElevator.Id,
                    optimalElevator.CurrentLoad,
                    optimalElevator.CurrentFloor,
                    optimalElevator.Status,
                    optimalElevator.Direction

                );

                elevatorInfo.UpdateCurrentLoad(optimalElevator.CurrentLoad + partialRequest.PeopleCount);
                elevatorInfo.EnqueueRequest(partialRequest);

                var elevator = _mapper.Map<Elevator>(elevatorInfo);
                await _unitOfWork.ElevatorRepository.UpdateAsync(elevator);
                await _unitOfWork.CompleteAsync();

                ElevatorRequest remainingRequest = new()
                {
                    Id = request.Id,
                    FromFloor = request.FromFloor,
                    ToFloor = request.ToFloor,
                    PeopleCount = request.PeopleCount - partialRequest.PeopleCount,
                    Direction = request.Direction
                };

                await _floorService.RequeuePartialRequestToFloorQueue(remainingRequest); // Add to overflow queue
                await _elevatorStateManager.FetchElevatorStateAsync(elevator.Id, elevatorInfo);

                return Response<ElevatorInfo>.Success("Partial load handled successfully.", elevatorInfo);
            }

            return Response<ElevatorInfo>.Failure("Partial load failed.");
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Loads passengers into the specified elevator, adjusting load and floor queue as necessary.
    /// </summary>
    public async Task<Response<ElevatorInfo>> LoadElevator(int elevatorId, ElevatorRequest request)
    {
        try
        {
            var elevatorResponse = await _unitOfWork.ElevatorRepository.FindByIdAsync(elevatorId);
            if (elevatorResponse == null)
                return Response<ElevatorInfo>.Failure($"Elevator {elevatorId} not found or unavailable.");

            var availableSpace = elevatorResponse.Capacity - elevatorResponse.CurrentLoad;
            if (request.PeopleCount <= availableSpace)
            {
                ElevatorInfo optimalElevator = new ElevatorInfo(
                    elevatorResponse.Id,
                    elevatorResponse.CurrentLoad,
                    elevatorResponse.CurrentFloor,
                    elevatorResponse.Status,
                    elevatorResponse.Direction

                );

                optimalElevator.UpdateCurrentLoad(optimalElevator.CurrentLoad + request.PeopleCount);
                optimalElevator.EnqueueRequest(request);

                var elevator = _mapper.Map<Elevator>(optimalElevator);
                await _unitOfWork.ElevatorRepository.UpdateAsync(elevator);
                await _unitOfWork.CompleteAsync();

                await _elevatorStateManager.FetchElevatorStateAsync(elevator.Id, optimalElevator);

                return Response<ElevatorInfo>.Success("Passengers loaded successfully.", optimalElevator);
            }

            return await HandlePartialLoad(elevatorResponse, request, availableSpace);

        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Offloads passengers from the specified elevator and updates its state.
    /// </summary>
    public async Task<Response<ElevatorInfo>> OffLoadElevator(int elevatorId, ElevatorRequest request)
    {
        try
        {
            var elevatorResponse = await _unitOfWork.ElevatorRepository.FindByIdAsync(elevatorId);
            if (elevatorResponse == null)
                return Response<ElevatorInfo>.Failure($"Elevator {elevatorId} not found or unavailable.");

            elevatorResponse.CurrentLoad = Math.Max(0, elevatorResponse.CurrentLoad - request.PeopleCount);

            ElevatorInfo optimalElevator = new ElevatorInfo(
                    elevatorResponse.Id,
                    elevatorResponse.CurrentLoad,
                    elevatorResponse.CurrentFloor,
                    elevatorResponse.Status,
                    elevatorResponse.Direction

                );

            optimalElevator.UpdateCurrentLoad(optimalElevator.CurrentLoad + request.PeopleCount);
            optimalElevator.EnqueueRequest(request);

            var elevator = _mapper.Map<Elevator>(optimalElevator);
            await _unitOfWork.ElevatorRepository.UpdateAsync(elevator);
            await _unitOfWork.CompleteAsync();

            await _elevatorStateManager.FetchElevatorStateAsync(elevatorId, optimalElevator);

            return Response<ElevatorInfo>.Success("Passengers offloaded successfully.", optimalElevator);
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// Resets the specified elevator to its default state.
    /// </summary>
    public async Task<Response<ElevatorInfo>> ResetElevator(int elevatorId, ElevatorInfo elevatorInfo)
    {
        try
        {
            elevatorInfo = new ElevatorInfo(
                    elevatorInfo.Id,
                    0,
                    0,
                    ElevatorStatus.Idle,
                    ElevatorDirection.Idle,
                    []

                );

            var elevator = _mapper.Map<Elevator>(elevatorInfo);
            await _unitOfWork.ElevatorRepository.UpdateAsync(elevator);
            await _unitOfWork.CompleteAsync();

            await _elevatorStateManager.FetchElevatorStateAsync(elevatorId, elevatorInfo);

            return Response<ElevatorInfo>.Success("Elevator reset successfully.", elevatorInfo);
        }
        catch (Exception)
        {
            throw;
        }
    }

    

    #region Private Methods



    #endregion


}
