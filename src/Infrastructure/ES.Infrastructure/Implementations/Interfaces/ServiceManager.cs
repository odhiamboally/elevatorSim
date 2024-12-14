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
    public IFloorQueueService FloorService { get; }
    public IFloorQueueManager FloorQueueManager { get; }
    public IQueueService QueueService { get; }

    public ServiceManager(

        //IElevatorCommand elevatorCommand,
        IKeyedServiceResolver<string, IElevatorCommand> elevatorCommandResolver,
        IElevatorService elevatorService, 
        IElevatorStateManager elevatorStateService, 
        IFloorQueueService floorService,
        IFloorQueueManager floorQueueManager,
        IQueueService queueService)
    {
        //ElevatorCommand = elevatorCommand;
        _elevatorCommandResolver = elevatorCommandResolver;
        ElevatorService = elevatorService;
        ElevatorStateManager = elevatorStateService;
        FloorService = floorService;
        FloorQueueManager = floorQueueManager;
        QueueService = queueService;
    }

    public IElevatorCommand GetElevatorCommand(string key) => _elevatorCommandResolver.Resolve(key);
}
