using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Abstractions.ICommands;
public interface IElevatorCommand
{
    Task ExecuteAsync();
}
