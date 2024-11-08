using ES.Application.Abstractions.ICommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Commands;

internal sealed class OpenDoorCommand : IElevatorCommand
{
    public OpenDoorCommand()
    {
            
    }

    public Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}
