using System.Text.Json.Serialization;

using Web.Client.Blazor.Enums;

namespace Web.Client.Blazor.Dtos;

public record ElevatorInfo
{
    private readonly object _lock = new();
    private const int MaxCapacity = 10;
    public int Id { get; init; }

    public int Capacity => MaxCapacity;

    // Private fields for mutable properties
    private int _currentFloor;
    private int _currentLoad;
    
    private ElevatorStatus _status = ElevatorStatus.Idle;
    private ElevatorDirection _direction = ElevatorDirection.Idle;

    public int CurrentFloor
    {
        get => _currentFloor;
        private set => _currentFloor = value;
    }

    public int CurrentLoad
    {
        get => _currentLoad;
        private set => _currentLoad = Math.Clamp(value, 0, MaxCapacity);
    }

    public ElevatorStatus Status
    {
        get => _status;
        private set => _status = value;
    }

    public ElevatorDirection Direction
    {
        get => _direction;
        private set => _direction = value;
    }

    public Queue<RequestInfo> RequestQueue { get; private set; } = [];

    public ElevatorInfo()
    {
            
    }

    [JsonConstructor]
    public ElevatorInfo(int id, int currentFloor, int currentLoad, ElevatorStatus status, ElevatorDirection direction, Queue<RequestInfo> requestQueue)
    {
        Id = id;
        CurrentFloor = currentFloor;
        CurrentLoad = currentLoad;
        Status = status;
        Direction = direction;
        RequestQueue = requestQueue;
    }

    //[JsonConstructor]
    public ElevatorInfo(int id, int currentFloor, int currentLoad, ElevatorStatus status, ElevatorDirection direction)
    {
        Id = id;
        CurrentFloor = currentFloor;
        CurrentLoad = currentLoad;
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
        lock (this)
        {
            _status = newStatus;
        }
    }

    /// <summary>
    /// Updates the direction of the elevator.
    /// </summary>
    /// <param name="newDirection">The new direction to set.</param>
    public void UpdateDirection(ElevatorDirection newDirection)
    {
        lock (this)
        {
            _direction = newDirection;
        }
    }

    /// <summary>
    /// Enqueues a request to the elevator's request queue.
    /// </summary>
    /// <param name="request">The request to enqueue.</param>
    public void EnqueueRequest(RequestInfo request)
    {
        lock (_lock)
        {
            RequestQueue.Enqueue(request);
        }
    }

    /// <summary>
    /// Dequeues a request from the elevator's request queue.
    /// </summary>
    public void DequeueRequest(RequestInfo request)
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
    public ElevatorInfo WithEnqueuedRequest(RequestInfo request)
    {
        var newQueue = new Queue<RequestInfo>(RequestQueue);
        newQueue.Enqueue(request);
        return this with { RequestQueue = newQueue };
    }

    // (Optimized): If immutability must be preserved
    public ElevatorInfo WithEnqueuedRequest_Optimized(RequestInfo request)
    {
        // Use a List for more efficient copying
        var newQueue = RequestQueue.ToList();
        newQueue.Add(request); // Add at the end
        return this with { RequestQueue = new Queue<RequestInfo>(newQueue) };
    }

    public ElevatorInfo WithEnqueuedRequest_Mutable(RequestInfo request)
    {
        RequestQueue.Enqueue(request); // Directly enqueue to the existing queue
        return this; // Return the same instance since the state is modified
    }

    // Dequeue a specific request
    public ElevatorInfo WithDequeuedRequest(RequestInfo request)
    {
        var newQueue = new Queue<RequestInfo>(RequestQueue.Where(r => r != request));
        return this with { RequestQueue = newQueue };
    }

    // (Optimized)
    public ElevatorInfo WithDequeuedRequest_Optimized(RequestInfo request)
    {
        var newQueue = RequestQueue.Where(r => r != request).ToList();
        return this with { RequestQueue = new Queue<RequestInfo>(newQueue) };
    }

    public ElevatorInfo WithDequeuedRequest_Mutable(RequestInfo request)
    {
        // Remove the specific request directly
        var removed = RequestQueue.TryDequeue(out var dequeuedItem);
        return this; // Return the same instance as state is modified
    }


    // Enqueue a request at a specific position (if necessary)
    public ElevatorInfo WithEnqueuedRequestAtPosition(RequestInfo request, int position)
    {
        var newQueue = new Queue<RequestInfo>(RequestQueue.Take(position));
        newQueue.Enqueue(request);
        newQueue = new Queue<RequestInfo>(newQueue.Concat(RequestQueue.Skip(position)));
        return this with { RequestQueue = newQueue };
    }

    public ElevatorInfo WithEnqueuedRequestAtPosition_Optimized(RequestInfo request, int position)
    {
        var newQueue = RequestQueue.ToList();
        newQueue.Insert(position, request);
        return this with { RequestQueue = new Queue<RequestInfo>(newQueue) };
    }


    // Dequeue a specific request by its ID (or any identifying property)
    public ElevatorInfo WithDequeuedRequestById(int requestId)
    {
        var newQueue = new Queue<RequestInfo>(RequestQueue.Where(r => r.Id != requestId));
        return this with { RequestQueue = newQueue };
    }

    public ElevatorInfo WithDequeuedRequestById_Optimized(int requestId)
    {
        var newQueue = RequestQueue.Where(r => r.Id != requestId).ToList();
        return this with { RequestQueue = new Queue<RequestInfo>(newQueue) };
    }

    #endregion

}

