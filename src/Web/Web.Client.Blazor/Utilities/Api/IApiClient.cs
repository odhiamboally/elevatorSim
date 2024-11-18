using Web.Client.Blazor.Dtos;

namespace Web.Client.Blazor.Utilities.Api;

public interface IApiClient
{
    Task<AccountInfo> FetchAccountData(AccountRequest request, string apiEndPoint);
    Task<List<ElevatorInfo>> FetchElevatorData(string apiEndPoint);
    Task<ElevatorInfo> RequestElevator(ElevatorRequest request, string apiEndPoint);
}
