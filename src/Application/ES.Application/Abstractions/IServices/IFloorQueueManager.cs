using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Abstractions.IServices;

public interface IFloorQueueManager
{
    Task<Response<Queue<ElevatorRequest>>> GetFloorQueue(int floorId);
    Task<Response<bool>> AddToQueue(int floorId, ElevatorRequest request);
    Task<Response<ElevatorRequest>>? DequeueRequest(int floorId);
}
