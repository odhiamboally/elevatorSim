using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Application.Abstractions.IServices;

public interface IFloorQueueService
{
    Task<Response<RequestInfo>> AddRequest(RequestInfo request);
    Task<Response<List<RequestInfo>>> GetAllRequests();
    Task<Response<List<RequestInfo>>> GetByElevatorId(RequestInfo request);
    Task<Response<List<RequestInfo>>> GetByFloorId(RequestInfo request);
    Task<Response<List<RequestInfo>>> GetByFloorIdThenByElevatorId(RequestInfo request);
    Task<Response<RequestInfo>> RemoveRequest(RequestInfo request);
    Task<Response<RequestInfo>> UpdateRequest(RequestInfo request);
    
    
}
