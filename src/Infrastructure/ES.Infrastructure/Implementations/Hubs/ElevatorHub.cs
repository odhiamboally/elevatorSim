using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Confluent.Kafka;

using ES.Application.Abstractions.Hubs;
using ES.Application.Dtos.Elevator;

using Microsoft.AspNetCore.SignalR;

namespace ES.Infrastructure.Implementations.Hubs;

internal sealed class ElevatorHub : Hub, IElevatorHub
{
    private readonly IHubContext<ElevatorHub> _hubContext;

    public ElevatorHub(IHubContext<ElevatorHub> hubContext)
    {
        _hubContext = hubContext;
            
    }

    public async Task BroadcastElevatorStateAsync(int elevatorId, ElevatorInfo updatedInfo)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveElevatorState", elevatorId, updatedInfo);
        }
        catch (Exception)
        {

            throw;
        }
    }
}
