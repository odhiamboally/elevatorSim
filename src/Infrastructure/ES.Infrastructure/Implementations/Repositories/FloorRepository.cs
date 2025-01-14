using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using ES.Application.Abstractions.IRepositories;
using ES.Domain.Entities;

namespace ES.Infrastructure.Implementations.Repositories;


internal sealed class FloorRepository : IFloorRepository
{
    public FloorRepository()
    {
            
    }

    public Task<Floor> CreateAsync(Floor entity)
    {
        throw new NotImplementedException();
    }

    public Task<Floor> DeleteAsync(Floor entity)
    {
        throw new NotImplementedException();
    }

    public IQueryable<Floor> FindAll()
    {
        throw new NotImplementedException();
    }

    public IQueryable<Floor> FindByCondition(Expression<Func<Floor, bool>> expression)
    {
        throw new NotImplementedException();
    }

    public Task<Floor?> FindByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<Floor> UpdateAsync(Floor entity)
    {
        throw new NotImplementedException();
    }
}
