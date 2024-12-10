namespace Web.Client.Blazor.Dtos;

public record CompleteRequest
{
    public ElevatorInfo? ElevatorInfo { get; init; }
    public ElevatorRequest? ElevatorRequest { get; init; }
}
