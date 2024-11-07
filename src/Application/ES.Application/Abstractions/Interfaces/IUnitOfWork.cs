using ES.Application.Abstractions.IRepositories;

namespace ES.Application.Abstractions.Interfaces;
public interface IUnitOfWork
{
    ILogRepository LogRepository { get; }

    Task<int> CompleteAsync();
}
