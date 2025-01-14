using ES.Application.Abstractions.ICommands;
using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IServices;
using ES.Infrastructure.Utilities;

namespace ES.Infrastructure.Implementations.Interfaces;
internal sealed class ServiceManager : IServiceManager
{
    //public IElevatorCommand ElevatorCommand { get; }
    private readonly IKeyedServiceResolver<string, IElevatorCommand> _elevatorCommandResolver;
    public IElevatorService ElevatorService { get; }
    public IElevatorStateManager ElevatorStateManager { get; }
    public IFloorService FloorService { get; }
    public IFloorQueueManager FloorQueueManager { get; }

    public ServiceManager(

        //IElevatorCommand elevatorCommand,
        IKeyedServiceResolver<string, IElevatorCommand> elevatorCommandResolver,
        IElevatorService elevatorService, 
        IElevatorStateManager elevatorStateService, 
        IFloorService floorService,
        IFloorQueueManager floorQueueManager)
    {
        //ElevatorCommand = elevatorCommand;
        _elevatorCommandResolver = elevatorCommandResolver;
        ElevatorService = elevatorService;
        ElevatorStateManager = elevatorStateService;
        FloorService = floorService;
        FloorQueueManager = floorQueueManager;
    }

    public IElevatorCommand GetElevatorCommand(string key) => _elevatorCommandResolver.Resolve(key);
}
