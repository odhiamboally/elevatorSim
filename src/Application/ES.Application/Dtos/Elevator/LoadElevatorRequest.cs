using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Dtos.Elevator;
public record LoadElevatorRequest
{
    public ElevatorInfo? ElevatorInfo { get; init; }
    public RequestInfo? ElevatorRequest { get; init; }
    public int PeopleCount { get; init; }
    public int RequestId { get; init; }
}
