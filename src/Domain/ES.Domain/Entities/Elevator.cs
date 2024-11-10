using ES.Domain.Enums;

namespace ES.Domain.Entities;


public class Elevator
{
    public int Id { get; }
    public int Capacity { get; } = 10;
    public int CurrentLoad { get; private set; } = 0;
    public int CurrentFloor { get; private set; } = 0;
    public Status Status { get; private set; }
    public Direction Direction { get; private set; }
    public Queue<int> RequestQueue { get; private set; } = [];




    // Method to update load based on new passengers loaded or offloaded
    public Elevator WithUpdatedLoad(int load)
    {
        return this with { CurrentLoad = Math.Clamp(load, 0, Capacity) };
    }

    // Method to set the elevator's movement direction
    public Elevator WithUpdatedDirection(Direction direction)
    {
        return this with { Direction = direction };
    }

    // Method to enqueue a new request
    public Elevator WithEnqueuedRequest(int requestId)
    {
        var newQueue = new Queue<int>(RequestQueue);
        newQueue.Enqueue(requestId);
        return this with { RequestQueue = newQueue };
    }

    // Method to dequeue the next request, if required
    public Elevator WithDequeuedRequest()
    {
        var newQueue = new Queue<int>(RequestQueue);
        if (newQueue.Count > 0)
        {
            newQueue.Dequeue();
        }
        return this with { RequestQueue = newQueue };
    }

}
