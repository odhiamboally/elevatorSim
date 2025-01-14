using ES.Domain.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Domain.Entities;

public class Request
{
    public int Id { get; set; }
    public int ElevatorId { get; set; }
    public int FromFloor { get; set; }
    public int ToFloor { get; set; }
    public int PeopleCount { get; set; }
    public ElevatorDirection Direction { get; set; }

    public Elevator? Elevator { get; set; }
}
