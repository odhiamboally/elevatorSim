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

    public LoadCommand()
    {
            
    }

    public async Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }

    
}
