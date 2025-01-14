using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ES.Application.Abstractions.Hubs;
using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IServices;

namespace ES.Infrastructure.Implementations.Interfaces;

internal sealed class HubManager : IHubManager
{
    public IElevatorHub ElevatorHub { get; }

    public HubManager(IElevatorHub elevatorHub)
    {
        ElevatorHub = elevatorHub;

    }
}
