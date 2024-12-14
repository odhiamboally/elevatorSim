namespace Web.Client.Blazor.Dtos;

public record OffloadRequest
{
    public RequestInfo? ElevatorRequest { get; init; }
    public ElevatorInfo? ElevatorInfo { get; init; }
}