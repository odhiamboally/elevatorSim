
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ES.Infrastructure.Implementations.Services;
internal sealed class ElevatorService : IElevatorService
{
    private readonly IConfiguration _config;
    private readonly IFloorService _floorService;
    private readonly IElevatorStateManager _elevatorStateManager;
    private readonly IFloorQueueManager _floorQueueManager;

    private readonly IUnitOfWork _unitOfWork;

    public ElevatorService(
        IConfiguration config, 
        IFloorService floorService, 
        IElevatorStateManager elevatorStateManager, 
        IFloorQueueManager floorQueueManager,
        IUnitOfWork unitOfWork)
    {
        _config = config;
        _floorService = floorService;
        _elevatorStateManager = elevatorStateManager;
        _floorQueueManager = floorQueueManager;
        _unitOfWork = unitOfWork;
        
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

            ElevatorInfo optimalElevator = new ElevatorInfo(
                elevatorForDispatch.Id,
                elevatorForDispatch.Capacity,
                elevatorForDispatch.CurrentLoad,
                elevatorForDispatch.CurrentFloor,
                elevatorForDispatch.Status,
                elevatorForDispatch.Direction

            );

            // Update the elevator state directly using encapsulated methods
            optimalElevator.UpdateStatus(ElevatorStatus.Moving);
            optimalElevator.UpdateDirection(optimalElevator.CurrentFloor < request.ToFloor ? ElevatorDirection.Up : ElevatorDirection.Down);
            optimalElevator.EnqueueRequest(request);

            await _elevatorStateManager.UpdateElevatorState(elevatorId, optimalElevator);

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
            var elevators = await _unitOfWork.ElevatorRepository.FindAll().ToListAsync();

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
                elevator.Capacity,
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

    public Task<Response<ElevatorInfo>> GetElevatorStatus(int elevatorId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Handles a partial load scenario when only part of a request can be loaded due to capacity limits.
    /// </summary>
    public async Task<Response<ElevatorInfo>> HandlePartialLoad(int elevatorId, ElevatorRequest request)
    {
        try
        {
            var elevatorResponse = await _unitOfWork.ElevatorRepository.FindByIdAsync(elevatorId);
            if (elevatorResponse == null)
                return Response<ElevatorInfo>.Failure("Elevator not found.");

            var elevator = elevatorResponse;
            var availableSpace = elevator.Capacity - elevator.CurrentLoad;

            if (availableSpace > 0)
            {
                var partialRequest = new ElevatorRequest
                {
                    Id = request.Id,
                    FromFloor = request.FromFloor,
                    ToFloor = request.ToFloor,
                    PeopleCount = Math.Min(request.PeopleCount, availableSpace),
                    Direction = request.Direction
                };

                ElevatorInfo optimalElevator = new ElevatorInfo(
                    elevator.Id,
                    elevator.Capacity,
                    elevator.CurrentLoad,
                    elevator.CurrentFloor,
                    elevator.Status,
                    elevator.Direction

                );

                elevator.CurrentLoad += partialRequest.PeopleCount;
                await UpdateElevatorQueue(optimalElevator, partialRequest);
                await _elevatorStateManager.UpdateElevatorState(elevatorId, optimalElevator);

                return Response<ElevatorInfo>.Success("Partial load handled successfully.", optimalElevator);
            }
            return Response<ElevatorInfo>.Failure("No space available for partial load.");
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

            var elevator = elevatorResponse;

            ElevatorInfo optimalElevator = new ElevatorInfo(
                    elevator.Id,
                    elevator.Capacity,
                    elevator.CurrentLoad,
                    elevator.CurrentFloor,
                    elevator.Status,
                    elevator.Direction

                );

            if (elevator.CurrentLoad + request.PeopleCount <= elevator.Capacity)
            {
                elevator.CurrentLoad += request.PeopleCount;
                await UpdateElevatorQueue(optimalElevator, request); // Queue this destination for service.
                await _elevatorStateManager.UpdateElevatorState(elevatorId, optimalElevator);

                return Response<ElevatorInfo>.Success("Passengers loaded successfully.", optimalElevator);
            }
            else
            {
                return Response<ElevatorInfo>.Failure("Elevator capacity exceeded.");
            }
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

            var elevator = elevatorResponse;
            elevator.CurrentLoad = Math.Max(0, elevator.CurrentLoad - request.PeopleCount);

            ElevatorInfo optimalElevator = new ElevatorInfo(
                    elevator.Id,
                    elevator.Capacity,
                    elevator.CurrentLoad,
                    elevator.CurrentFloor,
                    elevator.Status,
                    elevator.Direction

                );

            await _elevatorStateManager.UpdateElevatorState(elevatorId, optimalElevator);
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
    public async Task<Response<ElevatorInfo>> ResetElevator(int elevatorId, ElevatorInfo elevator)
    {
        try
        {
            elevator = new ElevatorInfo(
                    elevator.Id,
                    elevator.Capacity,
                    0,
                    0,
                    ElevatorStatus.Idle,
                    ElevatorDirection.Idle

                );

            await _elevatorStateManager.UpdateElevatorState(elevatorId, elevator);
            return Response<ElevatorInfo>.Success("Elevator reset successfully.", elevator);
        }
        catch (Exception ex)
        {
            return Response<ElevatorInfo>.Failure("Error resetting elevator.", null, ex);
        }
    }

    /// <summary>
    /// Updates the elevator queue with a new request destination.
    /// </summary>
    public async Task UpdateElevatorQueue(ElevatorInfo elevator, ElevatorRequest request)
    {
        await _elevatorStateManager.AddRequestToQueue(elevator.Id, request);
    }


}
