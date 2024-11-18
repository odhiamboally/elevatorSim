using Web.Client.Blazor.Enums;
using Web.Client.Blazor.Utilities.Common;

namespace Web.Client.Blazor.Dtos;

public record ElevatorRequest
{
    public int Id { get; init; } = IdGenerator.GetRequestNextId();
    public int FromFloor { get; init; }
    public int ToFloor { get; init; }
    public int PeopleCount { get; init; }
    public int Direction { get; init; }


    public ElevatorRequest(int fromFloor, int toFloor, int peopleCount, int direction)
    {
        FromFloor = fromFloor;
        ToFloor = toFloor;
        PeopleCount = peopleCount;
        Direction = direction;
    }

}
