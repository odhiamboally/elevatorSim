using ES.Domain.Enums;

namespace ES.Application.Dtos.Elevator;
public record RequestInfo
{

    private int _elevatorId;
    private int _peopleCount;

    public int Id { get; init; }

    public int ElevatorId
    {
        get => _elevatorId;
        private set => _elevatorId = value;
    }

    public int FromFloor { get; init; }
    public int ToFloor { get; init; }

    public int PeopleCount
    {
        get => _peopleCount;
        private set => _peopleCount = value;
    }

    public ElevatorDirection Direction { get; init; }


    public RequestInfo(int elevatorId, int fromFloor, int toFloor, int peopleCount, ElevatorDirection direction)
    {
        ElevatorId = elevatorId;
        FromFloor = fromFloor;
        ToFloor = toFloor;
        PeopleCount = peopleCount;
        Direction = direction;
    }

    public void UpdateElevatorId(int newElevatorId)
    {
        Interlocked.Exchange(ref _elevatorId, newElevatorId);
    }

    public void UpdatePeopleCount(int newCount)
    {
        Interlocked.Exchange(ref _peopleCount, newCount);
    }

}
