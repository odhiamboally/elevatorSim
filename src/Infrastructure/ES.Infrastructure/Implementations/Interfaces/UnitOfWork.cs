using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IRepositories;
using ES.Persistence.DataContext;

namespace ES.Infrastructure.Implementations.Interfaces;
public class UnitOfWork : IUnitOfWork
{
    public ILogRepository LogRepository { get; private set; }
    public IElevatorRepository ElevatorRepository { get; private set; }
    public IFloorQueueRepository FloorQueueRepository { get; private set; }

    private readonly DBContext _context;

    public UnitOfWork(
        ILogRepository logRepository,
        IElevatorRepository elevatorRepository,
        IFloorQueueRepository floorQueueRepository,
        DBContext context)
    {
        LogRepository = logRepository;
        ElevatorRepository = elevatorRepository;
        FloorQueueRepository = floorQueueRepository;
        _context = context;
    }



    public Task<int> CompleteAsync()
    {
        var result = _context.SaveChangesAsync();
        return result;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);

    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _context.Dispose();
        }
    }
}

