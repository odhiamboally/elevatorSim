using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Abstractions.IServices;

public interface IElevatorStateManager
{
    Task<Response<bool>> AddRequestToQueue(int id, ElevatorRequest request);
    Task<Response<bool>> UpdateElevatorState(int elevatorId, ElevatorInfo updatedInfo);
}
