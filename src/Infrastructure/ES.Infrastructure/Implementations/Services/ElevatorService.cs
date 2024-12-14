
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
using System.Text.Json;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ES.Infrastructure.Implementations.Services;
internal sealed class ElevatorService : IElevatorService
{
    private readonly IConfiguration _config;
    private readonly IElevatorStateManager _elevatorStateManager;
    private readonly IFloorQueueManager _floorQueueManager;
    private readonly IFloorQueueService _floorQueueService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ElevatorService(
        IConfiguration config, 
        IElevatorStateManager elevatorStateManager, 
        IFloorQueueManager floorQueueManager,
        IFloorQueueService floorQueueService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _config = config;
        _elevatorStateManager = elevatorStateManager;
        _floorQueueManager = floorQueueManager;
        _floorQueueService = floorQueueService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        
    }



    public async Task<Response<ElevatorInfo>> CompleteRequest(CompleteRequest request)
    {
        try
        {
            if (request.ElevatorInfo == null || request.ElevatorRequest == null)
                return Response<ElevatorInfo>.Failure("Elevator or Request not found.");

            var elevatorInfo = request.ElevatorInfo;
            var elevatorRequest = request.ElevatorRequest;

            elevatorInfo.DequeueRequest(elevatorRequest);

            // Process new boarding requests from the same floor
            var boardingRequests = await _floorQueueManager.GetFloorRequests(request.ElevatorInfo.CurrentFloor);
            if (boardingRequests.Successful && boardingRequests.Data!.Any())
            {
                foreach (var boardingRequest in boardingRequests.Data!)
                {
                    if (elevatorInfo.CurrentLoad + boardingRequest.PeopleCount <= elevatorInfo.Capacity)
                    {
                        var loadRequest = new LoadElevatorRequest
                        {
                            ElevatorInfo = elevatorInfo,
                            ElevatorRequest = boardingRequest,
                            PeopleCount = boardingRequest.PeopleCount,
                            RequestId = boardingRequest.Id,
                        };

                        await LoadElevator(loadRequest);
                    }
                }
            }

            if (!elevatorInfo.RequestQueue.Any())
            {
                elevatorInfo.UpdateStatus(ElevatorStatus.Idle);
                elevatorInfo.UpdateDirection(ElevatorDirection.Idle);
            }

            //elevatorInfo.UpdateCurrentFloor(request.ElevatorInfo.CurrentFloor);

            var updateStateResponse = await UpdateElevatorState(elevatorInfo);

            return updateStateResponse.Successful
                ? Response<ElevatorInfo>.Success("Request completed successfully.", updateStateResponse.Data!)
                : Response<ElevatorInfo>.Failure("Problem completing request.");
            
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<bool>> DispatchElevator(int elevatorId, RequestInfo request)
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

    public async Task<Response<ElevatorInfo>> DispatchElevator(DispatchElevatorRequest request)
    {
        try
        {
            if (request.ElevatorInfo == null)
                return Response<ElevatorInfo>.Failure("Elevator not found.");

            var elevatorInfo = request.ElevatorInfo;
            var elevatorRequest = request.ElevatorRequest;

            var newDirection = elevatorInfo.CurrentFloor < elevatorRequest!.ToFloor
                ? ElevatorDirection.Up
                : ElevatorDirection.Down;

            if (elevatorInfo.Direction != newDirection)
                elevatorInfo.UpdateDirection(newDirection);

            // Batch requests and reorder queue
            UpdateQueue(elevatorInfo, elevatorRequest);

            elevatorInfo.UpdateStatus(ElevatorStatus.Moving);

            var updateStateResponse = await UpdateElevatorState(elevatorInfo);

            return updateStateResponse.Successful
                ? Response<ElevatorInfo>.Success("Elevator dispatched successfully.", updateStateResponse.Data!)
                : Response<ElevatorInfo>.Failure("Elevator was not dispatched.");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<bool>> EnqueueRequestsToElevators()
    {
        try
        {
            var floorQueues = await _floorQueueService.GetAllRequests();
            if (!floorQueues.Data!.Any())
            {
                return Response<bool>.Failure("No requests found");
            }

            var bestElevator = new ElevatorInfo(0, 0, 0, 0, 0);
            bool enqueueResult = false;
            foreach (var request in floorQueues.Data!)
            {
                // Find the best elevator to serve this request
                var bestElevatorResponse = await FindElevator(request);
                if (bestElevatorResponse.Data != null)
                {
                    bestElevator = bestElevatorResponse.Data;
                    bestElevator.RequestQueue.Enqueue(request);
                    var updateStateResponse = await UpdateElevatorState(bestElevator);
                    
                }
            }

            enqueueResult = true;

            return enqueueResult == true
                ? Response<bool>.Success("Requests enqueued successfully.", true)
                : Response<bool>.Failure("Problem enqueuing requests.");

            //return Response<bool>.Success("All floor queues processed successfully.", true);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<ElevatorInfo>> FindElevator(RequestInfo request)
    {
        try
        {
            Queue<RequestInfo> queue = [];

            // Fetch all elevator states
            var elevators = _unitOfWork.ElevatorRepository.FindAll().ToList();

            if (elevators == null || !elevators.Any())
                return Response<ElevatorInfo>.Failure("No available elevator found.");

            // Primary selection: Elevators moving in the right direction or idle

            var elevator = elevators
                .Where(e => e.Status != ElevatorStatus.OutOfService) // Exclude out-of-service elevators
                .OrderBy(e => e.Status == ElevatorStatus.Idle ? 0 : 1) // Prefer idle elevators
                .ThenBy(e =>
                    e.Direction == request.Direction && // Prefer elevators moving in the same direction
                    ((request.Direction == ElevatorDirection.Up && e.CurrentFloor <= request.FromFloor) ||
                     (request.Direction == ElevatorDirection.Down && e.CurrentFloor >= request.FromFloor))
                    ? 0 : 1)
                .ThenBy(e => e.CurrentFloor == request.FromFloor ? 0 : 1) // Prefer elevators on the same floor
                .ThenBy(e =>
                    e.Direction == ElevatorDirection.Idle || // Handle idle elevators
                    (e.Direction != request.Direction && Math.Abs(e.CurrentFloor - request.FromFloor) <= 3)
                    ? 0 : 1) // Include opposite-direction elevators if close
                .ThenBy(e => Math.Abs(e.CurrentFloor - request.FromFloor)) // Closest elevators have higher priority
                .ThenBy(e => e.CurrentLoad + request.PeopleCount > e.Capacity ? 1 : 0) // Penalize elevators that can't accommodate
                .ThenBy(e => e.CurrentLoad / (double)e.Capacity) // Prefer elevators with more capacity available
                .ThenBy(e => EstimateTimeToReach(e, request.FromFloor, request)) // Use estimated time as a tie-breaker
                .FirstOrDefault();

            // Handle fallback logic if no eligible elevator is found
            if (elevator != null)
            {
                return Response<ElevatorInfo>.Success("Optimal elevator found.", new ElevatorInfo(
                    elevator.Id,
                    elevator.CurrentFloor,
                    elevator.CurrentLoad,
                    elevator.Status,
                    elevator.Direction,
                    queue
                
                ));
            }
            else
            {
                var eligibleElevators = elevators
                .Where(e => e.Status != ElevatorStatus.OutOfService) // Eliminate out-of-service elevators
                .Where(e => e.Direction == ElevatorDirection.Idle || // Idle elevators are always eligible
                            (e.Direction == request.Direction &&     // Elevators moving in the same direction
                             ((request.Direction == ElevatorDirection.Up && e.CurrentFloor <= request.FromFloor) ||
                              (request.Direction == ElevatorDirection.Down && e.CurrentFloor >= request.FromFloor))))
                .Where(e => e.CurrentLoad + request.PeopleCount <= e.Capacity || e.CurrentLoad < e.Capacity)
                .OrderBy(e => e.Status == ElevatorStatus.Idle ? 0 : 1) // Prioritize idle but consider distance
                .ThenBy(e => e.Status == ElevatorStatus.Idle ? Math.Abs(e.CurrentFloor - request.FromFloor) * 2 : Math.Abs(e.CurrentFloor - request.FromFloor)) // Penalize idle elevators for being far
                .ThenBy(e => Math.Abs(e.CurrentFloor - request.FromFloor)) // Closest first
                .ThenBy(e => e.CurrentLoad / (double)e.Capacity) // Least loaded
                .ToList();

                if (eligibleElevators == null)
                    return Response<ElevatorInfo>.Failure("No available elevator found.");

                if (!eligibleElevators.Any())
                {
                    // Fallback to elevators that can service after current requests
                    eligibleElevators = elevators
                        .Where(e => e.Status != ElevatorStatus.OutOfService)
                        .OrderBy(e => EstimateTimeToReach(e, request.FromFloor, request)) // Nearest by time
                        .ToList();
                }

                if (!eligibleElevators.Any())
                    return Response<ElevatorInfo>.Failure("No available elevator found.");

                var selectedElevator = eligibleElevators.First();

                var optimalElevator = new ElevatorInfo(
                    selectedElevator!.Id,
                    selectedElevator.CurrentFloor,
                    selectedElevator.CurrentLoad,
                    selectedElevator.Status,
                    selectedElevator.Direction,
                    queue

                );

                if (optimalElevator != null)
                {
                    return Response<ElevatorInfo>.Success("Primary Optimal Elevator found.", optimalElevator);
                }

                // Secondary selection: Elevators moving in the opposite direction
                var fallbackElevators = elevators
                    .Where(e => e.Direction == ElevatorDirection.Up && e.CurrentFloor > request.FromFloor ||
                                e.Direction == ElevatorDirection.Down && e.CurrentFloor < request.FromFloor)
                    .OrderBy(e => EstimateTimeToReach(e, request.FromFloor, request)) // Include time to complete current tasks
                    .ToList();

                if (fallbackElevators.Any())
                {
                    var optimalFallbackElevator = fallbackElevators.Select(fe => new ElevatorInfo(
                    selectedElevator!.Id,
                    selectedElevator.CurrentFloor,
                    selectedElevator.CurrentLoad,
                    selectedElevator.Status,
                    selectedElevator.Direction,
                    queue
                    )).First();

                    return optimalFallbackElevator != null
                        ? Response<ElevatorInfo>.Success("Fallback Elevator found.", optimalFallbackElevator!)
                        : Response<ElevatorInfo>.Failure($"No Fallback Elevator was found to handle request. Request ID : {request.Id}");
                }

                // Default to the least busy elevator if no match is found

                var leastBusyElevator = elevators.OrderBy(e => e.RequestQueue.Count).Select(fe => new ElevatorInfo(
                    selectedElevator!.Id,
                    selectedElevator.CurrentFloor,
                    selectedElevator.CurrentLoad,
                    selectedElevator.Status,
                    selectedElevator.Direction,
                    queue
                    )).FirstOrDefault();

                return leastBusyElevator != null
                    ? Response<ElevatorInfo>.Success("Least busy elevator found.", leastBusyElevator!)
                    : Response<ElevatorInfo>.Failure($"No Least busy elevator was found to handle request. Request ID : {request.Id}");
            }
            
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Response<ElevatorInfo>> FindElevatorOptimized(RequestInfo request)
    {
        try
        {
            // Fetch all elevator states
            var elevators = _unitOfWork.ElevatorRepository.FindAll().ToList();

            if (!elevators.Any())
                return Response<ElevatorInfo>.Failure("No available elevator found.");

            // Combine primary and secondary logic into one query
            var eligibleElevators = elevators
                .Where(e => e.Status != ElevatorStatus.OutOfService) // Eliminate out-of-service elevators
                .OrderBy(e =>
                {
                    if (e.Direction == ElevatorDirection.Idle)
                        return Math.Abs(e.CurrentFloor - request.FromFloor) * 2; // Penalize idle elevators for distance
                    if (e.Direction == request.Direction &&
                        ((request.Direction == ElevatorDirection.Up && e.CurrentFloor <= request.FromFloor) ||
                         (request.Direction == ElevatorDirection.Down && e.CurrentFloor >= request.FromFloor)))
                        return Math.Abs(e.CurrentFloor - request.FromFloor); // Favor elevators moving in the same direction
                    return EstimateTimeToReach(e, request.FromFloor, request); // Fallback to estimate time
                })
                .ThenBy(e => e.CurrentLoad / (double)e.Capacity) // Prioritize least loaded
                .ThenBy(e => e.RequestQueue.Count) // Least busy
                .ToList();

            // Select the best elevator from the eligible list
            var selectedElevator = eligibleElevators.FirstOrDefault();
            if (selectedElevator == null)
                return Response<ElevatorInfo>.Failure("No eligible elevator found.");

            var optimalElevator = new ElevatorInfo(
                selectedElevator.Id,
                selectedElevator.CurrentFloor,
                selectedElevator.CurrentLoad,
                selectedElevator.Status,
                selectedElevator.Direction,
                new Queue<RequestInfo>() // Initialize a fresh request queue
            );

            // Return the selected elevator
            return Response<ElevatorInfo>.Success("Elevator assigned successfully.", optimalElevator);
        }
        catch (Exception ex)
        {
            return Response<ElevatorInfo>.Failure($"An error occurred: {ex.Message}");
        }
    }

    public async Task<Response<ElevatorInfo>> LoadElevator(int elevatorId, RequestInfo request)
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

            return Response<ElevatorInfo>.Failure("There was a problem loading passengers.");

        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Response<ElevatorInfo>> LoadElevator(LoadElevatorRequest request)
    {
        try
        {
            if (request.ElevatorInfo == null)
                return Response<ElevatorInfo>.Failure("Elevator not found.");

            var elevatorInfo = request.ElevatorInfo;
            var elevatorRequest = request.ElevatorRequest;

            elevatorInfo.UpdateStatus(ElevatorStatus.Moving);
            //elevatorInfo.EnqueueRequest(elevatorRequest!);

            var newCurrentLoad = elevatorInfo.CurrentLoad + elevatorRequest!.PeopleCount;
            elevatorInfo.UpdateCurrentLoad(elevatorInfo.CurrentLoad + elevatorRequest.PeopleCount);
            elevatorInfo.UpdateDirection(elevatorInfo.CurrentFloor < elevatorRequest.ToFloor
            ? ElevatorDirection.Up
            : ElevatorDirection.Down);

            var updateStateRespose = await UpdateElevatorState(elevatorInfo);

            return updateStateRespose.Successful
                ? Response<ElevatorInfo>.Success("Elevator dispatched successfully.", updateStateRespose.Data!)
                : Response<ElevatorInfo>.Failure("Elevator was not dispatched.");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<ElevatorInfo>> OffloadElevator(OffloadRequest request)
    {
        try
        {
            if (request == null || request.ElevatorInfo == null || request.ElevatorRequest == null)
            {
                return Response<ElevatorInfo>.Failure("Invalid offload request.");
            }

            var elevatorInfo = request.ElevatorInfo;
            var elevatorRequest = request.ElevatorRequest;

            if (elevatorInfo.CurrentFloor != elevatorRequest.ToFloor)
                return Response<ElevatorInfo>.Failure("The elevator is not at the requested floor for offloading.");

            // Offload all passengers with the current floor as their destination
            var offloadRequests = elevatorInfo.RequestQueue
                .Where(r => r.ToFloor == elevatorInfo.CurrentFloor)
                .ToList();

            foreach (var offloadRequest in offloadRequests)
            {
                elevatorInfo.UpdateCurrentLoad(elevatorInfo.CurrentLoad - offloadRequest.PeopleCount);

            }

            var updateStateResponse = await UpdateElevatorState(elevatorInfo);

            return updateStateResponse.Successful
                ? Response<ElevatorInfo>.Success("Elevator offloaded successfully.", updateStateResponse.Data!)
                : Response<ElevatorInfo>.Failure("Elevator was not offloaded.");

        }
        catch (Exception)
        {

            throw;
        }
    }

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

    private int EstimateTimeToReach(Elevator elevator, int targetFloor, RequestInfo request)
    {
        int time = 0;
        var elevatorSpeed = TimeSpan.FromSeconds(2);
        
        var simulatedQueue = new Queue<int>(elevator.RequestQueue); // Deep copy: avoid mutating the original queue && race conditions

        // Simulate inserting the new request into the queue dynamically
        if (!simulatedQueue.Contains(request.FromFloor))
            simulatedQueue.Enqueue(request.FromFloor);

        if (!simulatedQueue.Contains(request.ToFloor))
            simulatedQueue.Enqueue(request.ToFloor);

        // Optimize stops in the queue
        var stopsInDirection = simulatedQueue
            .Where(floor =>
                (elevator.Direction == ElevatorDirection.Up && floor >= elevator.CurrentFloor) ||
                (elevator.Direction == ElevatorDirection.Down && floor <= elevator.CurrentFloor))
            .OrderBy(floor => Math.Abs(elevator.CurrentFloor - floor));

        var stopsOpposite = simulatedQueue
        .Except(stopsInDirection)
        .OrderBy(floor => Math.Abs(elevator.CurrentFloor - floor));

        simulatedQueue = new Queue<int>(stopsInDirection.Concat(stopsOpposite));

        int currentFloor = elevator.CurrentFloor;

        while (simulatedQueue.Any())
        {
            int nextStop = simulatedQueue.Dequeue();
             
            time += Math.Abs(currentFloor - nextStop) * elevatorSpeed.Seconds; // Add travel time

            time += 5; // Door operation time


            // Loading/unloading - More time for larger crowds (proportional to elevator capacity) - Can be made configurable

            if (nextStop == request.FromFloor || nextStop == request.ToFloor)
                time += request.PeopleCount <= elevator.Capacity / 2 ? 2 : 4; 

            // Stop simulation if the elevator reaches the target floor
            if (nextStop == targetFloor)
                break;

            currentFloor = nextStop;
        }

        // Add time to reach the target floor if not part of the queue
        if (currentFloor != targetFloor)
            time += Math.Abs(currentFloor - targetFloor) * elevatorSpeed.Seconds;

        return time;
    }

    private void UpdateQueue(ElevatorInfo elevatorInfo, RequestInfo newRequest)
    {
        var existingQueue = elevatorInfo.RequestQueue;

        // Insert source floor if not already in the queue
        if (!existingQueue.Any(r => r.FromFloor == newRequest.FromFloor))
        {
            existingQueue.Enqueue(newRequest); // Add the request to the queue
        }

        // Check if the destination floor is already in the queue
        if (!existingQueue.Any(r => r.ToFloor == newRequest.ToFloor))
        {
            existingQueue.Enqueue(new RequestInfo(
                newRequest.ElevatorId,
                newRequest.FromFloor,
                newRequest.ToFloor,
                newRequest.PeopleCount,
                newRequest.Direction

                ));
            
        }

        // Reorder the queue based on proximity to the current floor and direction
        elevatorInfo.ReOrderQueue(elevatorInfo);
    }

    private async Task<Response<ElevatorInfo>> UpdateElevatorState(ElevatorInfo elevatorInfo)
    {
        try
        {
            var elevator = new Elevator
            {
                Id = elevatorInfo.Id,
                Capacity = elevatorInfo.Capacity,
                CurrentFloor = elevatorInfo.CurrentFloor,
                CurrentLoad = elevatorInfo.CurrentLoad,
                Status = elevatorInfo.Status,
                Direction = elevatorInfo.Direction,
                RequestQueue = new Queue<int>(elevatorInfo.RequestQueue.Select(x => x.Id))
            };

            await _unitOfWork.ElevatorRepository.UpdateAsync(elevator);
            return await _elevatorStateManager.FetchElevatorStateAsync(elevatorInfo.Id, elevatorInfo);
        }
        catch (Exception)
        {

            throw;
        }
        
    }

    



    #endregion


}
