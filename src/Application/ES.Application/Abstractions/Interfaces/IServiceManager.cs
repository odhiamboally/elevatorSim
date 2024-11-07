using ES.Application.Abstractions.IServices;

namespace ES.Application.Abstractions.Interfaces;
public interface IServiceManager
{
    IElevatorService ElevatorService { get; }
    IElevatorStateManager ElevatorStateManager { get; }


}
