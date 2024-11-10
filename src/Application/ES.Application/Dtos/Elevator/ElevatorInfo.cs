using ES.Application.Utilities;
using ES.Domain.Enums;

namespace ES.Application.Dtos.Elevator;
//public record ElevatorInfo
//{
//    public int Id { get; } = RequestIdGenerator.GetElevatorNextId();
//    public int Capacity { get; } = 10;
//    public int CurrentLoad { get; private set; }
//    public int CurrentFloor { get; private set; }
//    public Status Status { get; private set; }
//    public Direction Direction { get; private set; }
//    public DateTimeOffset EstimatedArrivalTime { get; private set; }
//    public Queue<ElevatorRequest> RequestQueue { get; private set; } = [];



//    // Method to update load based on new passengers loaded or offloaded
//    public ElevatorInfo WithUpdatedLoad(int load)
//    {
//        return this with { CurrentLoad = Math.Clamp(load, 0, Capacity) };
//    }
//    // Method to change the elevator's floor
//    public ElevatorInfo WithUpdatedFloor(int floor)
//    {
//        return this with { CurrentFloor = floor };
//    }

//    // Method to update the elevator's status
//    public ElevatorInfo WithUpdatedStatus(Status status)
//    {
//        return this with { Status = status };
//    }

//    // Method to set the elevator's movement direction
//    public ElevatorInfo WithUpdatedDirection(Direction direction)
//    {
//        return this with { Direction = direction };
//    }

//    // Method to update the estimated arrival time
//    public ElevatorInfo WithUpdatedEstimatedArrival(DateTimeOffset estimatedArrival)
//    {
//        return this with { EstimatedArrivalTime = estimatedArrival };
//    }

//    // Method to enqueue a specific request
//    public ElevatorInfo WithEnqueuedRequest(ElevatorRequest request)
//    {
//        var newQueue = new Queue<ElevatorRequest>(RequestQueue);
//        newQueue.Enqueue(request);
//        return this with { RequestQueue = newQueue };
//    }

//    // Method to dequeue a specific request
//    public ElevatorInfo WithDequeuedRequest(ElevatorRequest request)
//    {
//        var newQueue = new Queue<ElevatorRequest>(RequestQueue.Where(r => r != request));
//        return this with { RequestQueue = newQueue };
//    }

//    // Method to enqueue a request at a specific position (if necessary)
//    public ElevatorInfo WithEnqueuedRequestAtPosition(ElevatorRequest request, int position)
//    {
//        var newQueue = new Queue<ElevatorRequest>(RequestQueue.Take(position));
//        newQueue.Enqueue(request);
//        newQueue = new Queue<ElevatorRequest>(newQueue.Concat(RequestQueue.Skip(position)));
//        return this with { RequestQueue = newQueue };
//    }

//    // Method to dequeue a specific request by its ID (or any identifying property)
//    public ElevatorInfo WithDequeuedRequestById(int requestId)
//    {
//        var newQueue = new Queue<ElevatorRequest>(RequestQueue.Where(r => r.Id != requestId));
//        return this with { RequestQueue = newQueue };
//    }

//}

public record ElevatorInfo
{
    public int Id { get; init;  } = RequestIdGenerator.GetElevatorNextId();
    public int Capacity { get; init; } = 10;
    public int CurrentLoad { get; init; }
    public int CurrentFloor { get; init; }
    public Status Status { get; init; }
    public Direction Direction { get; init; }
    public DateTimeOffset EstimatedArrivalTime { get; init; }
    public Queue<ElevatorRequest> RequestQueue { get; init; } = [];

}
