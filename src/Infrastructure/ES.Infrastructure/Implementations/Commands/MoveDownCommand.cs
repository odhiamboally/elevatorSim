using ES.Application.Abstractions.ICommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Commands;


internal sealed class MoveDownCommand : IElevatorCommand
{
    public MoveDownCommand()
    {
            
    }

    public Task ExecuteAsync()
    {
        throw new NotImplementedException();
    }


}
