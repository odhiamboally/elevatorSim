using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Enums;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Services;


internal sealed class QueueService : IQueueService
{
    private readonly ConcurrentDictionary<int, ConcurrentQueue<RequestInfo>> _floorQueues = [];

    public QueueService()
    {
        var queue1 = new ConcurrentQueue<RequestInfo>();
        queue1.Enqueue(new(2, 1, 2, 8, ElevatorDirection.Up) { Id = 2});
        queue1.Enqueue(new(2, 1, 6, 5, ElevatorDirection.Up) { Id = 5 });
        queue1.Enqueue(new(2, 1, 6, 5, ElevatorDirection.Up) { Id = 7 });
        queue1.Enqueue(new(4, 1, 2, 7, ElevatorDirection.Up) { Id = 8 });
        _floorQueues.TryAdd(1, queue1);

        var queue2 = new ConcurrentQueue<RequestInfo>();
        queue2.Enqueue(new(1, 1, 9, 2, ElevatorDirection.Up) { Id = 1 });
        queue2.Enqueue(new(1, 1, 4, 1, ElevatorDirection.Up) { Id = 4 });
        _floorQueues.TryAdd(3, queue2);

        var queue3 = new ConcurrentQueue<RequestInfo>();
        queue3.Enqueue(new(3, 9, 2, 4, ElevatorDirection.Down) { Id = 3 });
        queue3.Enqueue(new(3, 1, 2, 7, ElevatorDirection.Up) { Id = 6 });
        _floorQueues.TryAdd(6, queue3);


    }


    public async Task<Response<int>> AddRequestToFloorQueue(RequestInfo request)
    {
        try
        {
            if (request == null)
            {
                return Response<int>.Failure("Request cannot be null.");
            }

            if (!_floorQueues.ContainsKey(request.FromFloor))
            {
                _floorQueues[request.FromFloor] = new ConcurrentQueue<RequestInfo>();
            }

            if (!_floorQueues.TryGetValue(request.FromFloor, out var queue))
            {
                _floorQueues[request.FromFloor] = new ConcurrentQueue<RequestInfo>();
            }

            // Get or create a queue for the specified floor
            queue = _floorQueues.GetOrAdd(request.FromFloor, new ConcurrentQueue<RequestInfo>());
            queue.Enqueue(request);

            return Response<int>.Success("Request enqued successfully.", request.Id);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<ConcurrentQueue<RequestInfo>>> GetFloorQueue(int floorNumber)
    {
        try
        {
            if (_floorQueues.TryGetValue(floorNumber, out var queue))
            {
                return Response<ConcurrentQueue<RequestInfo>>.Success($"Floor no: {floorNumber}", new ConcurrentQueue<RequestInfo>(queue));
            }

            return Response<ConcurrentQueue<RequestInfo>>.Failure("Floor queue not found.");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<List<RequestInfo>>> GetFloorRequests(int floorNumber)
    {
        try
        {
            if (!_floorQueues.ContainsKey(floorNumber))
                return Response<List<RequestInfo>>.Failure($"No requests present on floor {floorNumber}");

            // Take a snapshot of the queue to safely iterate over it
            if (_floorQueues.TryGetValue(floorNumber, out var queue))
            {
                var requestsForFloor = queue.ToArray()
                    .Where(request => request.FromFloor == floorNumber)
                    .ToList();

                if (!requestsForFloor.Any())
                {
                    return Response<List<RequestInfo>>.Failure($"No matching requests found on floor {floorNumber}.");
                }

                return Response<List<RequestInfo>>.Success($"Floor {floorNumber} requests retrieved successfully.", requestsForFloor);
            }

            return Response<List<RequestInfo>>.Failure($"Failed to retrieve requests for floor {floorNumber}.");
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<List<RequestInfo>>> GetFloorRequestsByFloorNumberThenByElevatorId(int floorNumber, int elevatorId)
    {
        try
        {
            if (!_floorQueues.ContainsKey(floorNumber))
            {
                return Response<List<RequestInfo>>.Failure("No requests found for the specified floor.");
            }

            if (!_floorQueues.TryGetValue(floorNumber, out var queue))
            {
                return Response<List<RequestInfo>>.Failure("No requests found for the specified floor.");
            }

            // Lock the queue during enumeration to ensure thread safety
            var matchingRequests = new List<RequestInfo>();
            lock (queue)
            {
                foreach (var request in queue)
                {
                    if (request.ElevatorId == elevatorId)
                    {
                        matchingRequests.Add(request);
                    }
                }
            }

            if (!matchingRequests.Any())
            {
                return Response<List<RequestInfo>>.Failure("Requests not found for the specified elevator.");
            }

            return Response<List<RequestInfo>>.Success("Requests found: ", matchingRequests);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<int>> RemoveRequestFromFloorQueue(RequestInfo request)
    {
        try
        {
            if (!_floorQueues.ContainsKey(request.FromFloor))
            {
                return Response<int>.Failure("No requests found for the specified floor.");
            }

            if (!_floorQueues.TryGetValue(request.FromFloor, out var queue) || queue.Count == 0)
            {
                return Response<int>.Failure("No requests in queue or floor queue not found.", request.Id);
            }

            var updatedQueue = new ConcurrentQueue<RequestInfo>();
            bool found = false;

            // Dequeue each request, checking if it matches the target request
            while (queue.TryDequeue(out var currentRequest))
            {
                if (!found && AreRequestsEqual(currentRequest, request))
                {
                    found = true; // Skip adding this request to the updated queue
                }
                else
                {
                    updatedQueue.Enqueue(currentRequest);
                }
            }

            // Replace the old queue atomically
            _floorQueues[request.FromFloor] = updatedQueue;

            if (!found)
            {
                return Response<int>.Failure("Request not found in the queue.");
            }

            return Response<int>.Success("Request dequeued successfully.", request.Id);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public Task<Response<int>> RequeuePartialRequest(int fromFloor, ElevatorInfo data)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<int>> RequeuePartialRequestToFloorQueue(RequestInfo request)
    {
        try
        {
            if (!_floorQueues.TryGetValue(request.FromFloor, out var queue))
            {
                return Response<int>.Failure("Floor queue not found.");
            }

            // Requeuing a partially fulfilled request
            queue.Enqueue(request);
            return Response<int>.Success($"Requeued partial request : {request} for floor {request.FromFloor}", request.Id);
        }
        catch (Exception)
        {

            throw;
        }
    }

    private bool AreRequestsEqual(RequestInfo request1, RequestInfo request2)
    {
        return request1.Id == request2.Id;
    }



}
