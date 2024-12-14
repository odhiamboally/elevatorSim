using System.Security.Principal;

namespace Web.Client.Blazor.Dtos;

public record DispatchElevatorRequest
{
    public RequestInfo? ElevatorRequest  { get; init; }
    public ElevatorInfo? ElevatorInfo { get; init; }
}
