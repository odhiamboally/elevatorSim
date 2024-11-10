using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Abstractions.IServices;

public interface IFloorService
{
    Task<Response<Queue<ElevatorRequest>>> GetFloorQueue(int floorId);
    Task<Response<bool>> AddRequestToFloorQueue(ElevatorRequest request);
    Task<Response<bool>> RequeuePartialRequestToFloorQueue(ElevatorRequest request);
    Task<Response<bool>>? DequeueRequestFromFloorQueue(ElevatorRequest request);
}
