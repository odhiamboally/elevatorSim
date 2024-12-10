using ES.Shared.Exceptions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Web.Client.Blazor.Dtos;
using Web.Client.Blazor.Utilities.Api;
using Web.Client.Blazor.Utilities.SignalR;

namespace Web.Client.Blazor.Components.Pages;

public partial class Elevator : IAsyncDisposable
{
    private int FromFloor;
    private int Direction;
    private int ToFloor;
    private int PeopleCount;

    private bool isFormVisible = false;
    private bool isLoading = false;
    private string errorMessage = string.Empty;

    private ElevatorRequest? elevatorRequest;
    private List<ElevatorInfo> elevatorStates = new();
    private List<FloorInfo> Floors = new();

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private ISignalRService SignalRService { get; set; } = default!;

    [Inject]
    private IApiClient ApiClient { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        for (int i = 1; i <= 20; i++)
        {
            Floors.Add(new FloorInfo { Id = i });
        }

        SignalRService.ElevatorStateReceived += OnElevatorStateReceived;
        SignalRService.ElevatorStatesReceived += OnElevatorStatesReceived;

        await SignalRService.StartAsync();

        if (!elevatorStates.Any())
        {
            await LoadElevatorStatesAsync();
        }
    }

    private async Task LoadElevatorStatesAsync()
    {
        try
        {
            var apiEndPoint = Configuration["AppSettings:EndPoints:Elevator:GetElevatorStates"];
            if (string.IsNullOrEmpty(apiEndPoint))
                throw new ApiEndPointException("API endpoint is not configured.");

            elevatorStates = await ApiClient.FetchElevatorData(apiEndPoint);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            await JSRuntime.InvokeVoidAsync("Swal.fire", "Error", ex.Message, "error");
        }
    }

    private void OnElevatorStateReceived(int elevatorId, ElevatorInfo info)
    {
        InvokeAsync(() =>
        {
            var existingElevator = elevatorStates.FirstOrDefault(e => e.Id == elevatorId);
            if (existingElevator != null)
            {
                var index = elevatorStates.IndexOf(existingElevator);
                elevatorStates[index] = info;
                elevatorStates.RemoveAll(e => e.Id == elevatorId);
            }

            elevatorStates.Add(info);

            StateHasChanged();
        });
    }

    private void OnElevatorStatesReceived(List<KeyValuePair<int, ElevatorInfo>> elevatorInfos)
    {
        // Ensure that StateHasChanged is invoked on the correct Dispatcher thread
        InvokeAsync(() =>
        {
            elevatorStates = elevatorInfos.Select(e => e.Value).ToList();
            StateHasChanged();
        });
    }

    private void OpenRequestForm(int floor, int direction)
    {
        FromFloor = floor;
        Direction = direction;
        elevatorRequest = new ElevatorRequest(FromFloor, ToFloor, PeopleCount, Direction);
        isFormVisible = true;
    }

    private async Task HandleValidSubmit(EditContext context)
    {
        try
        {
            elevatorRequest = new ElevatorRequest(FromFloor, ToFloor, PeopleCount, Direction);
            var apiEndPoint = Configuration["AppSettings:EndPoints:Elevator:FindNearest"];
            elevatorStates.Add(await ApiClient.RequestElevator(elevatorRequest, apiEndPoint!));
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("Swal.fire", "Error", ex.Message, "error");
        }
        finally
        {
            isLoading = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Unsubscribe from SignalR events
        SignalRService.ElevatorStateReceived -= OnElevatorStateReceived;
        SignalRService.ElevatorStatesReceived -= OnElevatorStatesReceived;

        // Stop the SignalR connection
        await SignalRService.StopAsync();
    }
}
