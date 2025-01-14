using ES.Application.Abstractions.IRepositories;

namespace ES.Application.Abstractions.Interfaces;
public interface IUnitOfWork
{
    ILogRepository LogRepository { get; }
    IElevatorRepository ElevatorRepository { get; }
    IFloorQueueRepository FloorQueueRepository { get; }

    Task<int> CompleteAsync();
}
