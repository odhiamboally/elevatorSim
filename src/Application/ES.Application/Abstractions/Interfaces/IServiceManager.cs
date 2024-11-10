using ES.Application.Abstractions.ICommands;
using ES.Application.Abstractions.IServices;

namespace ES.Application.Abstractions.Interfaces;
public interface IServiceManager
{
    IElevatorCommand ElevatorCommand { get; }
    IElevatorService ElevatorService { get; }
    IElevatorStateManager ElevatorStateManager { get; }
    IFloorService FloorService { get; }
    IFloorQueueManager FloorQueueManager { get; }


}
