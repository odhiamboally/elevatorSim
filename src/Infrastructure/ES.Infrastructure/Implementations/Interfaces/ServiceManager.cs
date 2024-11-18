using ES.Application.Abstractions.ICommands;
using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IServices;

namespace ES.Infrastructure.Implementations.Interfaces;
internal sealed class ServiceManager : IServiceManager
{
    public IElevatorCommand ElevatorCommand { get; }
    public IElevatorService ElevatorService { get; }
    public IElevatorStateManager ElevatorStateManager { get; }
    public IFloorService FloorService { get; }
    public IFloorQueueManager FloorQueueManager { get; }

    public ServiceManager(
        IElevatorCommand elevatorCommand, 
        IElevatorService elevatorService, 
        IElevatorStateManager elevatorStateService, 
        IFloorService floorService,
        IFloorQueueManager floorQueueManager)
    {
        ElevatorCommand = elevatorCommand;
        ElevatorService = elevatorService;
        ElevatorStateManager = elevatorStateService;
        FloorService = floorService;
        FloorQueueManager = floorQueueManager;
    }
}
