using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Abstractions.IServices;

public interface IElevatorStateManager
{
    Task<Response<ElevatorInfo>> FetchElevatorStateAsync(int elevatorId, ElevatorInfo updatedInfo);
    Task<Response<List<ElevatorInfo>>> FetchElevatorStatesAsync();
    Task<Response<ElevatorInfo>> UpdateElevatorStateAsync(ElevatorInfo updatedInfo);
    Task<Response<ElevatorInfo>> UpdateElevatorStatesAsync(List<ElevatorInfo> elevatorInfos);
}
