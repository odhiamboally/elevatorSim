using Web.Client.Console.Enums;

namespace Web.Client.Console.Dtos;
internal sealed record ElevatorRequest
{
    public int RequestedFloor { get; init; }
    public int PeopleCount { get; init; }
    public Direction Direction { get; init; }

}
