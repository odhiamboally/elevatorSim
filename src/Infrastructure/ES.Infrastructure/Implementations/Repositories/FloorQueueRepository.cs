using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using ES.Application.Abstractions.IRepositories;
using ES.Domain.Entities;
using ES.Domain.Enums;
using ES.Shared.Exceptions;

namespace ES.Infrastructure.Implementations.Repositories;


internal sealed class FloorQueueRepository : IFloorQueueRepository
{
    private readonly ConcurrentDictionary<int, ConcurrentQueue<Request>> _floorQueues = [];

    public FloorQueueRepository()
    {
        var queue1 = new ConcurrentQueue<Request>();
        queue1.Enqueue(new Request
        {
            Id = 2,
            ElevatorId = 2,
            FromFloor = 1,
            ToFloor = 2,
            PeopleCount = 8,
            Direction = ElevatorDirection.Up,

        });

        queue1.Enqueue(new Request
        {
            Id = 5,
            ElevatorId = 2,
            FromFloor = 1,
            ToFloor = 6,
            PeopleCount = 5,
            Direction = ElevatorDirection.Up,

        });

        queue1.Enqueue(new Request
        {
            Id = 7,
            ElevatorId = 2,
            FromFloor = 1,
            ToFloor = 6,
            PeopleCount = 3,
            Direction = ElevatorDirection.Up,

        });

        queue1.Enqueue(new Request
        {
            Id = 8,
            ElevatorId = 4,
            FromFloor = 1,
            ToFloor = 2,
            PeopleCount = 7,
            Direction = ElevatorDirection.Up,

        });
        _floorQueues.TryAdd(1, queue1);

        var queue3 = new ConcurrentQueue<Request>();
        queue3.Enqueue(new Request
        {
            Id = 1,
            ElevatorId = 1,
            FromFloor = 1,
            ToFloor = 9,
            PeopleCount = 2,
            Direction = ElevatorDirection.Up,

        });

        queue3.Enqueue(new Request
        {
            Id = 4,
            ElevatorId = 1,
            FromFloor = 1,
            ToFloor = 4,
            PeopleCount = 1,
            Direction = ElevatorDirection.Up,

        });
        _floorQueues.TryAdd(3, queue3);

        var queue6 = new ConcurrentQueue<Request>();
        queue6.Enqueue(new Request
        {
            Id = 3,
            ElevatorId = 3,
            FromFloor = 9,
            ToFloor = 2,
            PeopleCount = 4,
            Direction = ElevatorDirection.Down,

        });

        queue6.Enqueue(new Request
        {
            Id = 6,
            ElevatorId = 3,
            FromFloor = 1,
            ToFloor = 2,
            PeopleCount = 7,
            Direction = ElevatorDirection.Up,

        });
        _floorQueues.TryAdd(6, queue6);

    }

    public async Task<Request> CreateAsync(Request entity)
    {
        if (!_floorQueues.TryGetValue(entity.FromFloor, out var queue))
        {
            queue = new ConcurrentQueue<Request>();
            _floorQueues.TryAdd(entity.FromFloor, queue);
        }

        queue.Enqueue(entity);
        return await Task.FromResult(entity);
    }

    public async Task<Request> DeleteAsync(Request entity)
    {
        if (_floorQueues.TryGetValue(entity.FromFloor, out var queue))
        {
            var items = queue.ToArray().Where(r => r.Id != entity.Id).ToList();
            _floorQueues[entity.FromFloor] = new ConcurrentQueue<Request>(items);
            return await Task.FromResult(entity);
        }

        throw new KeyNotFoundException($"No requests found for floor {entity.FromFloor}");
    }

    public IQueryable<Request> FindAll()
    {
        var allRequests = _floorQueues.Values.SelectMany(queue => queue).AsQueryable();
        return allRequests;
    }

    public IQueryable<Request> FindByCondition(Expression<Func<Request, bool>> expression)
    {
        var allRequests = FindAll();
        return allRequests.Where(expression);
    }

    public async Task<Request?> FindByIdAsync(int id)
    {
        var allRequests = _floorQueues.Values.SelectMany(queue => queue);
        var request = allRequests.FirstOrDefault(r => r.Id == id);
        return await Task.FromResult(request);
    }

    public async Task<Request> UpdateAsync(Request entity)
    {
        if (_floorQueues.TryGetValue(entity.FromFloor, out var queue))
        {
            var items = queue.ToArray();
            var updatedQueue = items
                .Select(r => r.Id == entity.Id ? entity : r)
                .ToList();

            _floorQueues[entity.FromFloor] = new ConcurrentQueue<Request>(updatedQueue);
            return await Task.FromResult(entity);
        }

        throw new KeyNotFoundException($"No requests found for floor {entity.FromFloor}");
    }
}
