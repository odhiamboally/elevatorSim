using System.Linq.Expressions;
using ES.Application.Abstractions.IRepositories;
using ES.Domain.Entities;

namespace ES.Infrastructure.Implementations.Repositories;
internal sealed class LogRepository : IBaseRepository<Log>, ILogRepository
{
    public Task<Log> CreateAsync(Log entity)
    {
        throw new NotImplementedException();
    }

    public Task<Log> DeleteAsync(Log entity)
    {
        throw new NotImplementedException();
    }

    public IQueryable<Log> FindAll()
    {
        throw new NotImplementedException();
    }

    public IQueryable<Log> FindByCondition(Expression<Func<Log, bool>> expression)
    {
        throw new NotImplementedException();
    }

    public Task<Log?> FindByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Log> UpdateAsync(Log entity)
    {
        throw new NotImplementedException();
    }
}
