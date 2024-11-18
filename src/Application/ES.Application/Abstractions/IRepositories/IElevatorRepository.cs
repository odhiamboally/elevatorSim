using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Entities;

namespace ES.Application.Abstractions.IRepositories;
public interface IElevatorRepository : IBaseRepository<Elevator>
{
    Task<Elevator> CreateToListAsync(Elevator entity);
    Task<Elevator> DeleteFromListAsync(Elevator entity);
    IQueryable<Elevator> FindAllFromList();
    IQueryable<Elevator> FindByConditionFromList(Expression<Func<Elevator, bool>> expression);
    Task<Elevator?> FindByIdFromListAsync(int id);
    Task<Elevator> UpdateToListAsync(Elevator entity);
}
