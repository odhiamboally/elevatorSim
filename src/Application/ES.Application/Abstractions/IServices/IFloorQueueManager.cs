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
    Task<Response<int>> AddRequestToFloor(int floorNumber, RequestInfo request);
    Task<Response<List<RequestInfo>>> GetFloorRequests(int floorNumber);
    Task<Response<bool>> RemoveRequestFromFloor(int floorNumber, int requestId);
    Task<Response<List<RequestInfo>>> GetFloorRequestsByFloorNumberThenByElevatorId(int floorNumber, int elevatorId);
    Task<Response<bool>> ProcessAllFloorQueues();
}
