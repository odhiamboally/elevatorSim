using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Dtos.Floor;
public record FloorRequest
{
    public int DestinationFloor { get; init; }
    public int PeopleCount { get; init; }
}
