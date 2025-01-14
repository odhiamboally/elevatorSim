using ES.Application.Utilities;
using ES.Domain.Enums;

namespace ES.Application.Dtos.Elevator;


public record ElevatorInfo
{
    private readonly object _lock = new();
    private const int MaxCapacity = 10;
    public int Id { get; init; }

    public int Capacity => MaxCapacity;

    // Private fields for mutable properties
    private int _currentLoad;
    private int _currentFloor;
    private int _status; // Internally store the status as an int (enum)
    private int _direction; // Internally store the direction as an int (enum)


    public int CurrentLoad
    {
        get => _currentLoad;
        private set => _currentLoad = Math.Clamp(value, 0, MaxCapacity);
    }

    public int CurrentFloor
    {
        get => _currentFloor;
        private set => _currentFloor = value;
    }

    public ElevatorStatus Status
    {
        get => (ElevatorStatus)_status;
        private set => _status = (int)value;
    }

    public ElevatorDirection Direction
    {
        get => (ElevatorDirection)_direction;
        private set => _direction = (int)value;
    }

    public Queue<ElevatorRequest> RequestQueue { get; private set; } = [];

    public ElevatorInfo(int id, int currentLoad, int currentFloor, ElevatorStatus status, ElevatorDirection direction, Queue<ElevatorRequest> requestQueue)
    {
        Id = id;
        CurrentLoad = currentLoad;
        CurrentFloor = currentFloor;
        Status = status;
        Direction = direction;
        RequestQueue = requestQueue;
    }

    public ElevatorInfo(int id, int currentLoad, int currentFloor, ElevatorStatus status, ElevatorDirection direction)
    {
        Id = id;
        CurrentLoad = currentLoad;
        CurrentFloor = currentFloor;
        Status = status;
        Direction = direction;
    }



    #region Mutable Methods

    // Update methods encapsulate logic and enforce rules

    /// <summary>
    /// Updates the current load of the elevator, clamping the value within valid bounds.
    /// </summary>
    /// <param name="newLoad">The new load to set.</param>
    public void UpdateCurrentLoad(int newLoad) 
    {
        newLoad = Math.Clamp(newLoad, 0, MaxCapacity);
        Interlocked.Exchange(ref _currentLoad, newLoad);
    }

    /// <summary>
    /// Updates the current floor of the elevator.
    /// </summary>
    /// <param name="newFloor">The new floor to set.</param>
    public void UpdateCurrentFloor(int newFloor) 
    {
        Interlocked.Exchange(ref _currentFloor, newFloor);
    }

    /// <summary>
    /// Updates the status of the elevator.
    /// </summary>
    /// <param name="newStatus">The new status to set.</param>
    public void UpdateStatus(ElevatorStatus newStatus)
    { 
        Interlocked.Exchange(ref _status, (int)newStatus); 
    }

    /// <summary>
    /// Updates the direction of the elevator.
    /// </summary>
    /// <param name="newDirection">The new direction to set.</param>
    public void UpdateDirection(ElevatorDirection newDirection)
    { 
        Interlocked.Exchange(ref _direction, (int)newDirection); 
    }

    /// <summary>
    /// Enqueues a request to the elevator's request queue.
    /// </summary>
    /// <param name="request">The request to enqueue.</param>
    public void EnqueueRequest(ElevatorRequest request)
    {
        lock (_lock)
        {
            RequestQueue.Enqueue(request);
        }
    }

    /// <summary>
    /// Dequeues a request from the elevator's request queue.
    /// </summary>
    public void DequeueRequest(ElevatorRequest request) 
    {
        lock (_lock)
        {
            RequestQueue.TryDequeue(out request!);
        }
    }

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
