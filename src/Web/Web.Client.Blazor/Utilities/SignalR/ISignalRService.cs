using Web.Client.Blazor.Dtos;

namespace Web.Client.Blazor.Utilities.SignalR;

public interface ISignalRService
{
    /// <summary>
    /// Starts the SignalR connection.
    /// </summary>
    Task StartAsync();
    Task StopAsync();
    ValueTask DisposeAsync();

    /// <summary>
    /// Event triggered when an elevator state is received.
    /// </summary>
    event Action<int, ElevatorInfo>? ElevatorStateReceived;
    event Action<List<KeyValuePair<int, ElevatorInfo>>>? ElevatorStatesReceived;

    /// <summary>
    /// Checks if the connection to SignalR is active.
    /// </summary>
    bool IsConnected { get; }
}
