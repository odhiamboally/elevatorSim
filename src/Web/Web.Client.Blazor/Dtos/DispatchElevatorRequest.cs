using System.Security.Principal;

namespace Web.Client.Blazor.Dtos;

public record DispatchElevatorRequest
{
    public ElevatorRequest? ElevatorRequest  { get; init; }
    public ElevatorInfo? ElevatorInfo { get; init; }
}
