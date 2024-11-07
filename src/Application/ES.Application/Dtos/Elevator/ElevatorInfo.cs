using ES.Domain.Enums;

namespace ES.Application.Dtos.Elevator;
public record ElevatorInfo
{
    public int Id { get; }
    public int Capacity { get; }
    public int CurrentLoad { get; private set; } = 0;
    public int CurrentFloor { get; private set; }
    public Status Status { get; init; }
    public Direction Direction { get; private set; }
    public DateTimeOffset EstimatedArrivalTime { get; private set; }
    
}
