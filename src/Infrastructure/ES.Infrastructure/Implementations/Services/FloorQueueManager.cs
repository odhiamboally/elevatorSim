using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Enums;
using ES.Shared.Exceptions;

namespace ES.Infrastructure.Implementations.Services;


internal sealed class FloorQueueManager : IFloorQueueManager
{

    private readonly IElevatorService _elevatorService;
    private readonly ConcurrentDictionary<int, List<RequestInfo>> _floorQueues = [];


    public FloorQueueManager()
    {
        //_elevatorService = elevatorService; ToDo: Is Causing Circular Dependency


        _floorQueues.TryAdd(1, new List<RequestInfo>
        {
            new (2, 1, 2, 8, ElevatorDirection.Up) { Id = 2 },
            new (2, 1, 6, 5, ElevatorDirection.Up) { Id = 5 },
            new (2, 1, 6, 5, ElevatorDirection.Up) { Id = 7 },
            new (4, 1, 2, 7, ElevatorDirection.Up) { Id = 8 }

        });

        _floorQueues.TryAdd(3, new List<RequestInfo>
        {
            new (1, 1, 9, 2, ElevatorDirection.Up) { Id = 1 },
            new (1, 1, 4, 1, ElevatorDirection.Up) { Id = 4 },

        });

        _floorQueues.TryAdd(6, new List<RequestInfo>
        {
            new (3, 9, 2, 4, ElevatorDirection.Down) { Id = 3 },
            new (3, 1, 2, 7, ElevatorDirection.Up) { Id = 6 }

        });
    }

    // Helper method to add a request (for testing or real-world usage)
    public async Task<Response<int>> AddRequestToFloor(int floorNumber, RequestInfo request)
    {
        if (request == null)
        {
            return Response<int>.Failure("Request cannot be null.");
        }

        if (!_floorQueues.ContainsKey(floorNumber))
        {
            _floorQueues[floorNumber] = new List<RequestInfo>();
        }

        _floorQueues[floorNumber].Add(request);
        return Response<int>.Success($"Added request no. {request.Id} to floor no. {floorNumber}", request.Id);
    }

    public async Task<Response<List<RequestInfo>>> GetFloorRequests(int floorNumber)
    {
        try
        {
            if (!_floorQueues.ContainsKey(floorNumber))
                return Response<List<RequestInfo>>.Failure($"No requests present on floor {floorNumber}");

            // Retrieve and filter requests specific to the elevator
            var requestsForFloor = _floorQueues[floorNumber]
                .Where(request => request.FromFloor == floorNumber)
                .ToList();

            // Optionally remove requests from the queue after retrieving
            _floorQueues[floorNumber] = _floorQueues[floorNumber]
                .Except(requestsForFloor)
                .ToList();

            return Response<List<RequestInfo>>.Success($"Foor {floorNumber} requests: {requestsForFloor.Count}", requestsForFloor);
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

            var requests = _floorQueues[floorNumber]
                .Where(request => request.ElevatorId == elevatorId || request.ElevatorId == 0 && request.FromFloor == floorNumber)
                .ToList();

            if (!requests.Any())
            {
                return Response<List<RequestInfo>>.Failure("Request not found.");
            }

            return Response<List<RequestInfo>>.Success("Requests found: ", requests);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<bool>> ProcessAllFloorQueues()
    {
        try
        {
            foreach (var floorQueue in _floorQueues)
            {
                var floorNumber = floorQueue.Key;
                var requests = floorQueue.Value;

                foreach (var request in requests)
                {
                    // Find the best elevator to serve this request
                    var bestElevator = await _elevatorService.FindElevator(request);

                    if (bestElevator != null)
                    {
                        bestElevator.Data!.RequestQueue.Enqueue(request);
                    }
                }
            }

            return Response<bool>.Success("All floor queues processed successfully.", true);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<bool>> RemoveRequestFromFloor(int floorNumber, int requestId)
    {
        try
        {
            if (!_floorQueues.ContainsKey(floorNumber))
            {
                return Response<bool>.Failure("No requests found for the specified floor.");
            }

            var queue = _floorQueues[floorNumber];
            var request = queue.SingleOrDefault(r => r.Id == requestId);

            if (request == null)
            {
                return Response<bool>.Failure("Request not found.");
            }

            queue.Remove(request);

            return Response<bool>.Success("Request deleted successfully.", true);
        }
        catch (Exception)
        {

            throw;
        }
    }

}
