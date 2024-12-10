using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Dtos.Elevator;
public record DispatchElevatorRequest
{
    public ElevatorRequest? ElevatorRequest { get; init; }
    public ElevatorInfo? ElevatorInfo { get; init; }
}
