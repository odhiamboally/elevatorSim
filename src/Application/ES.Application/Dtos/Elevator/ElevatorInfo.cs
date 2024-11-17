using ES.Application.Utilities;
using ES.Domain.Enums;

namespace ES.Application.Dtos.Elevator;


public record ElevatorInfo
{
    public int Id { get; private set; } = RequestIdGenerator.GetElevatorNextId();
    public int Capacity { get; } = 10;
    public int CurrentLoad { get; private set; }
    public int CurrentFloor { get; private set; }
    public ElevatorStatus Status { get; private set; }
    public ElevatorDirection Direction { get; private set; }
    public Queue<ElevatorRequest> RequestQueue { get; private set; } = [];

    public ElevatorInfo(int id, int capacity, int currentLoad, int currentFloor, ElevatorStatus status, ElevatorDirection direction)
    {
        Id = id;
        Capacity = capacity;
        CurrentLoad = currentLoad;
        CurrentFloor = currentFloor;
        Status = status;
        Direction = direction;
    }



    #region Mutable Methods

    // Update methods encapsulate logic and enforce rules
    public void UpdateId(int newId) => Id = newId;
    public void UpdateCurrentLoad(int load) => CurrentLoad = Math.Clamp(load, 0, Capacity);
    public void UpdateCurrentFloor(int floor) => CurrentFloor = floor;
    public void UpdateStatus(ElevatorStatus status) => Status = status;
    public void UpdateDirection(ElevatorDirection direction) => Direction = direction;

    public void EnqueueRequest(ElevatorRequest request) => RequestQueue.Enqueue(request);
    public void DequeueRequest() => RequestQueue.TryDequeue(out _);

    #endregion

    #region Immutable "With-Style" Methods

    // Update load based on new passengers loaded or offloaded
    public ElevatorInfo WithUpdatedLoad(int load)
    {
        return this with { CurrentLoad = Math.Clamp(load, 0, Capacity) };
    }

    public ElevatorInfo WithUpdatedFloor(int floor)
    {
        return this with { CurrentFloor = floor };
    }

    public ElevatorInfo WithUpdatedStatus(ElevatorStatus status)
    {
        return this with { Status = status };
    }

    public ElevatorInfo WithUpdatedDirection(ElevatorDirection direction)
    {
        return this with { Direction = direction };
    }

    // Enqueue a specific request
    public ElevatorInfo WithEnqueuedRequest(ElevatorRequest request)
    {
        var newQueue = new Queue<ElevatorRequest>(RequestQueue);
        newQueue.Enqueue(request);
        return this with { RequestQueue = newQueue };
    }

    // (Optimized): If immutability must be preserved
    public ElevatorInfo WithEnqueuedRequest_Optimized(ElevatorRequest request)
    {
        // Use a List for more efficient copying
        var newQueue = RequestQueue.ToList();
        newQueue.Add(request); // Add at the end
        return this with { RequestQueue = new Queue<ElevatorRequest>(newQueue) };
    }

    public ElevatorInfo WithEnqueuedRequest_Mutable(ElevatorRequest request)
{
    RequestQueue.Enqueue(request); // Directly enqueue to the existing queue
    return this; // Return the same instance since the state is modified
}

    // Dequeue a specific request
    public ElevatorInfo WithDequeuedRequest(ElevatorRequest request)
    {
        var newQueue = new Queue<ElevatorRequest>(RequestQueue.Where(r => r != request));
        return this with { RequestQueue = newQueue };
    }

    // (Optimized)
    public ElevatorInfo WithDequeuedRequest_Optimized(ElevatorRequest request)
    {
        var newQueue = RequestQueue.Where(r => r != request).ToList();
        return this with { RequestQueue = new Queue<ElevatorRequest>(newQueue) };
    } 

    public ElevatorInfo WithDequeuedRequest_Mutable(ElevatorRequest request)
    {
        // Remove the specific request directly
        var removed = RequestQueue.TryDequeue(out var dequeuedItem);
        return this; // Return the same instance as state is modified
    }


    // Enqueue a request at a specific position (if necessary)
    public ElevatorInfo WithEnqueuedRequestAtPosition(ElevatorRequest request, int position)
    {
        var newQueue = new Queue<ElevatorRequest>(RequestQueue.Take(position));
        newQueue.Enqueue(request);
        newQueue = new Queue<ElevatorRequest>(newQueue.Concat(RequestQueue.Skip(position)));
        return this with { RequestQueue = newQueue };
    }

    public ElevatorInfo WithEnqueuedRequestAtPosition_Optimized(ElevatorRequest request, int position)
    {
        var newQueue = RequestQueue.ToList();
        newQueue.Insert(position, request);
        return this with { RequestQueue = new Queue<ElevatorRequest>(newQueue) };
    }


    // Dequeue a specific request by its ID (or any identifying property)
    public ElevatorInfo WithDequeuedRequestById(int requestId)
    {
        var newQueue = new Queue<ElevatorRequest>(RequestQueue.Where(r => r.Id != requestId));
        return this with { RequestQueue = newQueue };
    }

    public ElevatorInfo WithDequeuedRequestById_Optimized(int requestId)
    {
        var newQueue = RequestQueue.Where(r => r.Id != requestId).ToList();
        return this with { RequestQueue = new Queue<ElevatorRequest>(newQueue) };
    }

    #endregion

}

public record ElevatorInfo1
{
    public int Id { get; init;  } = RequestIdGenerator.GetElevatorNextId();
    public int Capacity { get; init; } = 10;
    public int CurrentLoad { get; init; }
    public int CurrentFloor { get; init; }
    public ElevatorStatus Status { get; init; }
    public ElevatorDirection Direction { get; init; }
    public DateTimeOffset EstimatedArrivalTime { get; init; }
    public Queue<ElevatorRequest> RequestQueue { get; init; } = [];

}
