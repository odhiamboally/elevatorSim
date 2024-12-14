using ES.Application.Abstractions.ICommands;
using ES.Application.Abstractions.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Commands;

internal sealed class CloseDoorCommand : IElevatorCommand
{

    public CloseDoorCommand()
    {

    }
    public Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }
}
