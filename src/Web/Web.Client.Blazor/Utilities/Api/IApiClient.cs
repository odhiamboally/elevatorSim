using Web.Client.Blazor.Dtos;

namespace Web.Client.Blazor.Utilities.Api;

public interface IApiClient
{
    Task<AccountInfo> FetchAccountData(AccountRequest request, string apiEndPoint);
    Task<List<ElevatorInfo>> FetchElevatorData(string apiEndPoint);
    Task<ElevatorInfo> RequestElevator(ElevatorRequest request, string apiEndPoint);
    Task<ElevatorInfo> DispatchElevator(ElevatorRequest elevatorRequest, string apiEndPoint);
    Task<ElevatorInfo> DispatchElevator(DispatchElevatorRequest dispatchElevatorRequest, string apiEndPoint);
    Task<ElevatorInfo> CompleteRequest(ElevatorRequest elevatorRequest, string apiEndPoint);
    Task<ElevatorInfo> CompleteRequest(CompleteRequest completeRequest, string apiEndPoint);
}
