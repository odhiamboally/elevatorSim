using ES.Application.Utilities;
using ES.Domain.Enums;

namespace ES.Application.Dtos.Elevator;
public record ElevatorRequest
{
    public int Id { get; init; } = RequestIdGenerator.GetRequestNextId();
    public int FromFloor { get; init; }
    public int ToFloor { get; init; }
    public int PeopleCount { get; init; }
    public Direction Direction { get; init; }
    
}
