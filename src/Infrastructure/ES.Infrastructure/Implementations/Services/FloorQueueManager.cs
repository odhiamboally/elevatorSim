using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Services;


internal sealed class FloorQueueManager : IFloorQueueManager
{
    private readonly Dictionary<int, Queue<ElevatorRequest>> _floorQueues = new();

    public FloorQueueManager()
    {
            
    }

    public async Task<Response<bool>> AddToQueue(int floorId, ElevatorRequest request)
    {
        try
        {
            var response = await GetFloorQueue(floorId);
            if(response.Data == null)
            {
                return Response<bool>.Failure("No Queue on the specified floor");
            }

            return Response<bool>.Success("Request successfully added to queue", true);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<ElevatorRequest>>? DequeueRequest(int floorId)
    {
        try
        {
            var request = await GetFloorQueue(floorId);

            if (request.Data != null)
                request.Data.Dequeue();

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<Queue<ElevatorRequest>>> GetFloorQueue(int floorId)
    {
        try
        {
            if (!_floorQueues.ContainsKey(floorId))
                _floorQueues[floorId] = new Queue<ElevatorRequest>();

            var floorQueue = _floorQueues[floorId];

            return Response<Queue<ElevatorRequest>>.Success("Floor Queue", floorQueue);
        }
        catch (Exception)
        {

            throw;
        }
    }
}
