using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Abstractions.IServices;


public interface IQueueService
{
    Task<Response<int>> AddRequestToFloorQueue(RequestInfo request);
    Task<Response<ConcurrentQueue<RequestInfo>>> GetFloorQueue(int floorNumber);
    Task<Response<List<RequestInfo>>> GetFloorRequests(int floorNumber);
    Task<Response<List<RequestInfo>>> GetFloorRequestsByFloorNumberThenByElevatorId(int floorNumber, int elevatorId);
    Task<Response<int>> RemoveRequestFromFloorQueue(RequestInfo request);
    Task<Response<int>> RequeuePartialRequest(int fromFloor, ElevatorInfo data);
    Task<Response<int>> RequeuePartialRequestToFloorQueue(RequestInfo request);
}
