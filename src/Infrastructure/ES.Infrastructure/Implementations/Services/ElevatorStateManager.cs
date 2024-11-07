using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Services;
internal sealed class ElevatorStateManager : IElevatorStateManager
{

    public ElevatorStateManager()
    {
            
    }

    public Task<Response<ElevatorInfo>> FindAvailableElevator(int requestedFloor, Direction direction)
    {
        throw new NotImplementedException();
    }

    public Task<Response<ElevatorInfo>> GetElevatorState(int elevatorId)
    {
        throw new NotImplementedException();
    }

    public Task<Response<bool>> UpdateElevatorState(int elevatorId, ElevatorInfo updatedInfo)
    {
        throw new NotImplementedException();
    }
}
