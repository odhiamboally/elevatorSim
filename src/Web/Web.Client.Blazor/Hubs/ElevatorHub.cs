using Microsoft.AspNetCore.SignalR;

using Web.Client.Blazor.Dtos;

namespace Web.Client.Blazor.Hubs;

public class ElevatorHub : Hub
{
    public ElevatorHub()
    {
            
    }

    public async Task SendElevatorState(int elevatorId, ElevatorInfo elevatorInfo)
    {
        await Clients.All.SendAsync("ReceiveElevatorState", elevatorId, elevatorInfo);
    }
}
