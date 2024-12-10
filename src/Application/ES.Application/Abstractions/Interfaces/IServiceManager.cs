using ES.Application.Abstractions.ICommands;
using ES.Application.Abstractions.IServices;

namespace ES.Application.Abstractions.Interfaces;
public interface IServiceManager
{
    //IElevatorCommand ElevatorCommand { get; }

    IElevatorService ElevatorService { get; }
    IElevatorStateManager ElevatorStateManager { get; }
    IFloorService FloorService { get; }
    IFloorQueueManager FloorQueueManager { get; }


    /// <summary>
    /// Resolves a specific ElevatorCommand by key.
    /// </summary>
    /// <param name="key">The key identifying the command.</param>
    /// <returns>The resolved elevator command.</returns>
    IElevatorCommand GetElevatorCommand(string key);

}
