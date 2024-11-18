using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ES.Application.Abstractions.Hubs;
using ES.Application.Abstractions.ICommands;

namespace ES.Application.Abstractions.Interfaces;
public interface IHubManager
{
    IElevatorHub ElevatorHub { get; }
}
