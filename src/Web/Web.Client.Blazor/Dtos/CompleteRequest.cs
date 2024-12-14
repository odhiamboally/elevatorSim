namespace Web.Client.Blazor.Dtos;

public record CompleteRequest
{
    public ElevatorInfo? ElevatorInfo { get; init; }
    public RequestInfo? ElevatorRequest { get; init; }
}
