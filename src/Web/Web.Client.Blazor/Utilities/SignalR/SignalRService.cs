
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

using Web.Client.Blazor.Dtos;

namespace Web.Client.Blazor.Utilities.SignalR;

internal sealed class SignalRService : ISignalRService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SignalRService> _logger;
    private readonly HubConnection _hubConnection;

    public event Action<int, ElevatorInfo>? ElevatorStateReceived;
    public event Action<List<KeyValuePair<int, ElevatorInfo>>>? ElevatorStatesReceived;
    public bool IsConnected => _hubConnection.State == HubConnectionState.Connected;

    public SignalRService(IConfiguration configuration, NavigationManager navigationManager, ILogger<SignalRService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var hubUrl = navigationManager.ToAbsoluteUri($"{_configuration["AppSettings:ApiBaseUrl"]}{_configuration["AppSettings:EndPoints:Elevator:Hub"]}");
        var absoluteUri = navigationManager.ToAbsoluteUri($"{hubUrl}");

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(absoluteUri)
            .WithAutomaticReconnect()
            .Build();

        _logger.LogInformation("Hub URL: {HubUrl}", hubUrl);

        _hubConnection.On<int, ElevatorInfo>("ReceiveElevatorState", (elevatorId, info) =>
        {
            ElevatorStateReceived?.Invoke(elevatorId, info);
        });

        _hubConnection.On<List<KeyValuePair<int, ElevatorInfo>>>("ReceiveElevatorStates", (states) =>
        {
            ElevatorStatesReceived?.Invoke(states);
        });

        // Register for lifecycle events
        _hubConnection.Closed += OnClosed;
        _hubConnection.Reconnected += OnReconnected;
        _hubConnection.Reconnecting += OnReconnecting;
    }

    public async Task StartAsync()
    {
        await _hubConnection.StartAsync();
    }

    private Task OnClosed(Exception? exception)
    {
        _logger.LogWarning(exception, "SignalR connection closed.");
        return Task.CompletedTask;
    }

    private Task OnReconnected(string? connectionId)
    {
        _logger.LogInformation("SignalR connection reconnected with Connection ID: {ConnectionId}", connectionId);
        return Task.CompletedTask;
    }

    private Task OnReconnecting(Exception? exception)
    {
        _logger.LogWarning(exception, "SignalR connection is reconnecting...");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        await _hubConnection.StopAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
