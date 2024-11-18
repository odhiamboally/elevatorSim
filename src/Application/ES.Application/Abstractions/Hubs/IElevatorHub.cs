using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ES.Application.Dtos.Elevator;


namespace ES.Application.Abstractions.Hubs;

public interface IElevatorHub
{
    Task BroadcastElevatorStateAsync(int elevatorId, ElevatorInfo updatedInfo);
}
