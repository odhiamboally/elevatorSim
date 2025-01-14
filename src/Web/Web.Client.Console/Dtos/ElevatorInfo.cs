using Web.Client.Console.Enums;

namespace Web.Client.Console.Dtos;


internal sealed record ElevatorInfo
{
    public int Id { get; }
    public int Capacity { get; }
    public int CurrentLoad { get; private set; }
    public int CurrentFloor { get; private set; }
    public Status Status { get; init; }
    public Direction Direction { get; private set; }
    public DateTimeOffset EstimatedArrivalTime { get; private set; }
    private Queue<ElevatorRequest> RequestQueue { get; } = [];


}
