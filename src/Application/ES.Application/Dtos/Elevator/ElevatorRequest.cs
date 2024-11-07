using ES.Domain.Enums;

namespace ES.Application.Dtos.Elevator;
public record ElevatorRequest
{
    public int RequestedFloor { get; init; }
    public int PeopleCount { get; init; }
    public Direction Direction { get; init; }
    
}
