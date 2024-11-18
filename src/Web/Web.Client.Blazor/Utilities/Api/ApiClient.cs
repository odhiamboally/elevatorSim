using Web.Client.Blazor.Dtos;

namespace Web.Client.Blazor.Utilities.Api;

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("ES");
    }

    public async Task<AccountInfo> FetchAccountData(AccountRequest request, string apiEndPoint)
    {
        try
        {
            var apiResponse = await _httpClient.PostAsJsonAsync(apiEndPoint, request);
            if (!apiResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error calling API: {apiResponse.StatusCode}");
            }
            return await apiResponse.Content.ReadFromJsonAsync<AccountInfo>()
                ?? throw new InvalidOperationException("Invalid response from API.");
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
            return await apiResponse.Content.ReadFromJsonAsync<List<ElevatorInfo>>()
                ?? throw new InvalidOperationException("Invalid response from API.");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<ElevatorInfo> RequestElevator(ElevatorRequest request, string apiEndPoint)
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
}
