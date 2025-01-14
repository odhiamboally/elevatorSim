using ES.Domain.Enums;

namespace ES.Domain.Entities;


public class Elevator
{
    public int Id { get; set; }
    public int Capacity { get; set; }
    public int CurrentLoad { get; set; }
    public int CurrentFloor { get; set; }
    public ElevatorStatus Status { get; set; }
    public ElevatorDirection Direction { get; set; }
    public Queue<int> RequestQueue { get; set; } = [];
    public virtual List<Request>? Requests { get; set; }

}
