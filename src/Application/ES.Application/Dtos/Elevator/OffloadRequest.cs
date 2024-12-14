using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Dtos.Elevator;
public record OffloadRequest
{
    public RequestInfo? ElevatorRequest { get; init; }
    public ElevatorInfo? ElevatorInfo { get; init; }
}

