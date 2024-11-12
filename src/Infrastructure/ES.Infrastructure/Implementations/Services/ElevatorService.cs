
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
using Microsoft.Extensions.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ES.Infrastructure.Implementations.Services;
internal sealed class ElevatorService : IElevatorService
{
    private readonly IConfiguration _config;
    private readonly IFloorService _floorService;
    private readonly IElevatorStateManager _elevatorStateManager;
    private readonly IFloorQueueManager _floorQueueManager;

    public ElevatorService(
        IConfiguration config, 
        IFloorService floorService, 
        IElevatorStateManager elevatorStateManager, 
        IFloorQueueManager floorQueueManager)
    {
        _config = config;
        _floorService = floorService;
        _elevatorStateManager = elevatorStateManager;
        _floorQueueManager = floorQueueManager;
        
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
            var elevatorResponse = await _elevatorStateManager.GetElevatorState(elevatorId);
            if (!elevatorResponse.Successful || elevatorResponse.Data == null)
                return Response<bool>.Failure("Elevator not found.");

            var elevator = elevatorResponse.Data;
            //elevator = new ElevatorInfo()
            //    .WithUpdatedStatus(Status.Moving)
            //    .WithUpdatedDirection(elevator.CurrentFloor < request.ToFloor ? Direction.Up : Direction.Down);

            var dispatchedElevator = new ElevatorInfo
            {
                Id = elevatorId,
                Capacity = elevator.Capacity,
                CurrentFloor = elevator.CurrentFloor,
                CurrentLoad = elevator.CurrentLoad,
                Status = Status.Moving,
                Direction = elevator.CurrentFloor < request.ToFloor ? Direction.Up : Direction.Down,
                EstimatedArrivalTime = elevator.EstimatedArrivalTime,
                RequestQueue = elevator.RequestQueue,
            };

            await _elevatorStateManager.UpdateElevatorState(elevatorId, dispatchedElevator);

            //await _elevatorStateManager.UpdateElevatorState(elevatorId, elevator);
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
            var elevator = await FindBestFitElevator(request);
            return elevator != null
                ? Response<ElevatorInfo>.Success("Nearest elevator found", elevator)
                : Response<ElevatorInfo>.Failure("No available elevator found.");
        }
        catch (Exception ex)
        {
            return Response<ElevatorInfo>.Failure("Error finding nearest elevator.", null, ex);
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
            var elevatorResponse = await _elevatorStateManager.GetElevatorState(elevatorId);
            if (!elevatorResponse.Successful || elevatorResponse.Data == null)
                return Response<ElevatorInfo>.Failure("Elevator not available.");

            var elevator = elevatorResponse.Data;
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

                elevator.CurrentLoad += partialRequest.PeopleCount;
                await UpdateElevatorQueue(elevator, partialRequest);
                await _elevatorStateManager.UpdateElevatorState(elevatorId, elevator);

                return Response<ElevatorInfo>.Success("Partial load handled successfully.", elevator);
            }
            return Response<ElevatorInfo>.Failure("No space available for partial load.");
        }
        catch (Exception ex)
        {
            return Response<ElevatorInfo>.Failure("Error handling partial load.", null, ex);
        }
    }

    /// <summary>
    /// Loads passengers into the specified elevator, adjusting load and floor queue as necessary.
    /// </summary>
    public async Task<Response<ElevatorInfo>> LoadElevator(int elevatorId, ElevatorRequest request)
    {
        try
        {
            var elevatorResponse = await _elevatorStateManager.GetElevatorState(elevatorId);
            if (!elevatorResponse.Successful || elevatorResponse.Data == null)
                return Response<ElevatorInfo>.Failure($"Elevator {elevatorId} not found or unavailable.");

            var elevator = elevatorResponse.Data;

            if (elevator.CurrentLoad + request.PeopleCount <= elevator.Capacity)
            {
                elevator.CurrentLoad += request.PeopleCount;
                await UpdateElevatorQueue(elevator, request); // Queue this destination for service.
                await _elevatorStateManager.UpdateElevatorState(elevatorId, elevator);

                return Response<ElevatorInfo>.Success("Passengers loaded successfully.", elevator);
            }
            else
            {
                return Response<ElevatorInfo>.Failure("Elevator capacity exceeded.");
            }
        }
        catch (Exception ex)
        {
            return Response<ElevatorInfo>.Failure("Error loading elevator.", null, ex);
        }
    }

    /// <summary>
    /// Offloads passengers from the specified elevator and updates its state.
    /// </summary>
    public async Task<Response<ElevatorInfo>> OffLoadElevator(int elevatorId, ElevatorRequest request)
    {
        try
        {
            var elevatorResponse = await _elevatorStateManager.GetElevatorState(elevatorId);
            if (!elevatorResponse.Successful || elevatorResponse.Data == null)
                return Response<ElevatorInfo>.Failure("Elevator not found.");

            var elevator = elevatorResponse.Data;
            elevator.CurrentLoad = Math.Max(0, elevator.CurrentLoad - request.PeopleCount);

            await _elevatorStateManager.UpdateElevatorState(elevatorId, elevator);
            return Response<ElevatorInfo>.Success("Passengers offloaded successfully.", elevator);
        }
        catch (Exception ex)
        {
            return Response<ElevatorInfo>.Failure("Error offloading elevator.", null, ex);
        }
    }

    /// <summary>
    /// Resets the specified elevator to its default state.
    /// </summary>
    public async Task<Response<ElevatorInfo>> ResetElevator(int elevatorId, ElevatorInfo elevator)
    {
        try
        {
            elevator.CurrentFloor = 0;
            elevator.CurrentLoad = 0;
            elevator.Status = Status.Idle;
            elevator.Direction = Direction.Idle;

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

    private async Task<ElevatorInfo?> FindBestFitElevator(ElevatorRequest request)
    {
        // This would involve logic to locate the most optimal elevator based on factors like distance and current state.
        return await _elevatorStateManager.FindOptimalElevator(request);
    }


}
