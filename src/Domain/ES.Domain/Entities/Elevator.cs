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
    private Queue<int> RequestQueue { get; } = [];
}
