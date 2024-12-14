namespace Web.Client.Blazor.Dtos;

public record LoadElevatorRequest
{
    public RequestInfo? ElevatorRequest { get; init; }
    public ElevatorInfo? ElevatorInfo { get; init; }
    public int PeopleCount { get; init; }
    public int RequestId { get; init; }
}
