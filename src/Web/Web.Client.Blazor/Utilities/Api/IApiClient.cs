using Web.Client.Blazor.Dtos;

namespace Web.Client.Blazor.Utilities.Api;

public interface IApiClient
{
    Task<int> AddRequestToFloorQueue(RequestInfo elevatorRequest, int floorNumber, string apiEndPoint);
    Task<ElevatorInfo> CompleteRequest(CompleteRequest completeRequest, string apiEndPoint);
    Task<ElevatorInfo> DispatchElevator(RequestInfo elevatorRequest, string apiEndPoint);
    Task<ElevatorInfo> DispatchElevator(DispatchElevatorRequest dispatchElevatorRequest, string apiEndPoint);
    Task<bool> EnqueueRequestsToElevators(string apiEndPoint);
    Task<List<ElevatorInfo>> FetchElevatorData(string apiEndPoint);
    Task<ElevatorInfo> FindElevator(RequestInfo request, string apiEndPoint);
    Task<ElevatorInfo> LoadElevator(LoadElevatorRequest loadElevatorRequest, string apiEndPoint);
    Task<ElevatorInfo> OffloadElevator(OffloadRequest request, string apiEndPoint);
    Task<ElevatorInfo> UpdateElevatorStateAsync(ElevatorInfo updatedInfo, string apiEndPoint);


}
