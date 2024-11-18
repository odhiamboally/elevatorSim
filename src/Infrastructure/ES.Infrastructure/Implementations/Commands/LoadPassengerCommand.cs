using ES.Application.Abstractions.ICommands;
using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Elevator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Commands;


internal class LoadPassengerCommand : IElevatorCommand
{
    private readonly IServiceManager _serviceManager;
    private readonly ElevatorRequest _request;

    public LoadPassengerCommand(IServiceManager serviceManager, ElevatorRequest elevatorRequest)
    {
        _serviceManager = serviceManager;
        _request = elevatorRequest;
            
    }

    public async Task ExecuteAsync()
    {
        var result = await _serviceManager.ElevatorService.LoadElevator(elevatorId, _request);
        if (!result.Successful && result.Data != null)
        {
            await _serviceManager.ElevatorStateManager.BroadcastStateAsync(elevatorId, result.Data);
        }
    }
}
