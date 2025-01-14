using Web.Client.Console.Dtos;

namespace Web.Client.Console.ApiClients;

internal interface IApiClient
{
    Task<ElevatorInfo> FindNearestElevator(ElevatorRequest request, string apiEndPoint);
    Task<ElevatorInfo> DispatchElevator(ElevatorRequest request, string apiEndPoint);
}
