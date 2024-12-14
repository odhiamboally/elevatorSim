using ES.Shared.Exceptions;

using System.Net.Http;
using System.Text.Json;

using Web.Client.Blazor.Dtos;
using Web.Client.Blazor.Enums;

namespace Web.Client.Blazor.Utilities.Api;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ES");
    }

    public async Task<int> AddRequestToFloorQueue(RequestInfo elevatorRequest, int floorNumber, string apiEndPoint)
    {
        try
        {
            var fullApiEndPoint = $"{apiEndPoint}{floorNumber}";
            var apiResponse = await _httpClient.PostAsJsonAsync(fullApiEndPoint, elevatorRequest);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }

            apiResponse.EnsureSuccessStatusCode();
            var requestId = await apiResponse.Content.ReadFromJsonAsync<int>();
            return requestId != 0
                ? requestId
                : throw new InvalidOperationException("API returned null for CompleteRequest.");

        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("Error while completing the request.", ex);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ElevatorInfo> CompleteRequest(CompleteRequest completeRequest, string apiEndPoint)
    {
        try
        {
            var apiResponse = await _httpClient.PostAsJsonAsync(apiEndPoint, completeRequest);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }

            apiResponse.EnsureSuccessStatusCode();
            return await apiResponse.Content.ReadFromJsonAsync<ElevatorInfo>()
                ?? throw new InvalidOperationException("API returned null for CompleteRequest.");

        }
        catch (HttpRequestException ex)
        {
            throw new ApiException("Error while completing the request.", ex);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ElevatorInfo> DispatchElevator(RequestInfo elevatorRequest, string apiEndPoint)
    {
        try
        {
            var apiResponse = await _httpClient.PostAsJsonAsync(apiEndPoint, elevatorRequest);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }
            return await apiResponse.Content.ReadFromJsonAsync<ElevatorInfo>()
                ?? throw new InvalidOperationException("Invalid response from API.");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ElevatorInfo> DispatchElevator(DispatchElevatorRequest dispatchElevatorRequest, string apiEndPoint)
    {
        try
        {
            var apiResponse = await _httpClient.PostAsJsonAsync(apiEndPoint, dispatchElevatorRequest);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }

            var contentResponse = await apiResponse.Content.ReadAsStringAsync();

            return await apiResponse.Content.ReadFromJsonAsync<ElevatorInfo>()
                ?? throw new InvalidOperationException("Invalid response from API.");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<bool> EnqueueRequestsToElevators(string apiEndPoint)
    {
        try
        {
            var apiResponse = await _httpClient.GetAsync(apiEndPoint);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }

            var contentResponse = apiResponse.Content.ReadAsStringAsync();

            return await apiResponse.Content.ReadFromJsonAsync<bool>()
                ? true
                : throw new InvalidOperationException("Invalid response from API.");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<List<ElevatorInfo>> FetchElevatorData(string apiEndPoint)
    {
        try
        {
            var apiResponse = await _httpClient.GetAsync(apiEndPoint);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }

            var contentResponse = apiResponse.Content.ReadAsStringAsync();

            return await apiResponse.Content.ReadFromJsonAsync<List<ElevatorInfo>>()
                ?? throw new InvalidOperationException("Invalid response from API.");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ElevatorInfo> FindElevator(RequestInfo request, string apiEndPoint)
    {
        try
        {
            var apiResponse = await _httpClient.PostAsJsonAsync(apiEndPoint, request);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }

            return await apiResponse.Content.ReadFromJsonAsync<ElevatorInfo>()
                ?? throw new InvalidOperationException("Invalid response from API.");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<ElevatorInfo> LoadElevator(LoadElevatorRequest loadElevatorRequest, string apiEndPoint)
    {
        try
        {
            var apiResponse = await _httpClient.PostAsJsonAsync(apiEndPoint, loadElevatorRequest);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }

            var contentResponse = await apiResponse.Content.ReadAsStringAsync();

            return await apiResponse.Content.ReadFromJsonAsync<ElevatorInfo>()
                ?? throw new InvalidOperationException("Invalid response from API.");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ElevatorInfo> OffloadElevator(OffloadRequest request, string apiEndPoint)
    {
        try
        {
            var apiResponse = await _httpClient.PostAsJsonAsync(apiEndPoint, request);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }

            return await apiResponse.Content.ReadFromJsonAsync<ElevatorInfo>()
                ?? throw new InvalidOperationException("Invalid response from API.");
        }
        catch (JsonException)
        {
            throw;
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ElevatorInfo> UpdateElevatorStateAsync(ElevatorInfo updatedInfo, string apiEndPoint)
    {
        try
        {
            var apiResponse = await _httpClient.PostAsJsonAsync(apiEndPoint, updatedInfo);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }

            var contentResponse = await apiResponse.Content.ReadAsStringAsync();

            return await apiResponse.Content.ReadFromJsonAsync<ElevatorInfo>()
                ?? throw new InvalidOperationException("Invalid response from API.");
        }
        catch (Exception)
        {

            throw;
        }
    }
}
