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


internal class LoadCommand : IElevatorCommand
{
    private readonly IServiceManager _serviceManager;

    public LoadCommand(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
            
    }

    public async Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }

    
}
