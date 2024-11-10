using ES.Application.Dtos.Elevator;

namespace ES.Application.Dtos.Floor;

public record FloorInfo
{
    public int Id { get; init; }
    public int PeopleCount { get; init; }
    public Queue<ElevatorRequest> Requests { get; set; } = [];

}
