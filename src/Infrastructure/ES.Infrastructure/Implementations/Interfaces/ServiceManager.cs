using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IServices;

namespace ES.Infrastructure.Implementations.Interfaces;
internal sealed class ServiceManager : IServiceManager
{
    public IElevatorService ElevatorService { get; }
    public IElevatorStateManager ElevatorStateManager { get; }

    public ServiceManager(IElevatorService elevatorService, IElevatorStateManager elevatorStateManager)
    {
        ElevatorService = elevatorService;
        ElevatorStateManager = elevatorStateManager;
    }
}
