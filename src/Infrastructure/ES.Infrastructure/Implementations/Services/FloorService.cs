using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using MassTransit.Internals;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Services;


internal sealed class FloorService : IFloorService
{
    private readonly ConcurrentDictionary<int, Queue<ElevatorRequest>> _floorQueues;

    public FloorService()
    {
        _floorQueues = new ConcurrentDictionary<int, Queue<ElevatorRequest>>();
    }

    public async Task<Response<bool>> AddRequestToFloorQueue(ElevatorRequest request)
    {
        try
        {
            // Get or create a queue for the specified floor
            var queue = _floorQueues.GetOrAdd(request.FromFloor, new Queue<ElevatorRequest>());

            // Add the request to the queue
            queue.Enqueue(request);

            return Response<bool>.Success("Request enqued successfully.", true);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<bool>> DequeueRequestFromFloorQueue(ElevatorRequest request)
    {
        try
        {
            if (!_floorQueues.TryGetValue(request.FromFloor, out var queue) || queue.Count == 0)
            {
                return Response<bool>.Failure("No requests in queue or floor queue not found.", false);
            }

            bool found = false;
            var tempQueue = new Queue<ElevatorRequest>();

            // Dequeue each request, checking if it matches the target request
            while (queue.Count > 0)
            {
                var currentRequest = queue.Dequeue();
                if (!found && AreRequestsEqual(currentRequest, request))
                {
                    found = true; // Skip adding this request back into the temp queue
                }
                else
                {
                    tempQueue.Enqueue(currentRequest);
                }
            }

            // Restore the remaining requests back into the original queue
            while (tempQueue.Count > 0)
            {
                queue.Enqueue(tempQueue.Dequeue());
            }

            return Response<bool>.Success("Request dequeued successfully.", true);
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
            if (_floorQueues.TryGetValue(floorId, out var queue))
            {
                return Response<Queue<ElevatorRequest>>.Success($"Floor no: {floorId}", queue);
            }
            return Response<Queue<ElevatorRequest>>.Failure("Floor queue not found.", null);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<bool>> RequeuePartialRequestToFloorQueue(ElevatorRequest request)
    {
        try
        {
            if (!_floorQueues.TryGetValue(request.FromFloor, out var queue))
            {
                return Response<bool>.Failure("Floor queue not found.");
            }

            // Logic for requeuing a partially fulfilled request
            queue.Enqueue(request);
            return Response<bool>.Success($"Requeued partial request : {request} for floor {request.FromFloor}", true);
        }
        catch (Exception)
        {

            throw;
        }
    }

    private bool AreRequestsEqual(ElevatorRequest request1, ElevatorRequest request2)
    {
        return request1.Id == request2.Id; 
    }
}
