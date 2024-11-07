using Web.Client.Console.Dtos;

namespace Web.Client.Console.ApiClients;

internal interface IApiClient
{
    Task<ElevatorInfo> CallElevator(ElevatorRequest request, string apiEndPoint);
}
