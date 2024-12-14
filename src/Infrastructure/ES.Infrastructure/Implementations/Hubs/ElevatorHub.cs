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

public class ElevatorHub : Hub, IElevatorHub
{
    

    public ElevatorHub()
    {
            
    }

    public async Task FetchElevatorStateAsync(int elevatorId, ElevatorInfo updatedInfo)
    {
        try
        {
            await Clients.All.SendAsync("ReceiveElevatorState", elevatorId, updatedInfo);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task FetchElevatorStatesAsync(List<ElevatorInfo> elevatorStates)
    {
        try
        {
            await Clients.All.SendAsync("ReceiveElevatorStates", elevatorStates);
        }
        catch (Exception)
        {

            throw;
        }
    }
}
