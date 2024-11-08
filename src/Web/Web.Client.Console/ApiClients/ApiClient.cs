using System.Net.Http.Json;
using Web.Client.Console.Dtos;

namespace Web.Client.Console.ApiClients;


internal sealed class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;


    public ApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ES");
    }

    public async Task<ElevatorInfo> FindNearestElevator(ElevatorRequest request, string apiEndPoint)
    {
        var apiResponse = await _httpClient.PostAsJsonAsync(apiEndPoint, request);
        if (!apiResponse.IsSuccessStatusCode)
        {

        }

        var elevatorResponse = await apiResponse.Content.ReadFromJsonAsync<ElevatorInfo>();
        return elevatorResponse!;
    }

    public async Task<ElevatorInfo> DispatchElevator(ElevatorRequest request, string apiEndPoint)
    {
        var apiResponse = await _httpClient.PostAsJsonAsync(apiEndPoint, request);
        if (!apiResponse.IsSuccessStatusCode)
        {

        }

        var accountInfoResponse = await apiResponse.Content.ReadFromJsonAsync<ElevatorInfo>();
        return accountInfoResponse!;
    }

}
