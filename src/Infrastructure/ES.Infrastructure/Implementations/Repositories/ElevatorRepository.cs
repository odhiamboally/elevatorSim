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
using ES.Shared.Utilities;

namespace ES.Infrastructure.Implementations.Repositories;


internal sealed class ElevatorRepository : IElevatorRepository
{
    private readonly List<Elevator> _elevatorList;
    private readonly ConcurrentDictionary<int, Elevator> _elevators = [];

    public ElevatorRepository()
    {
        _elevatorList = new List<Elevator>
        {
            new Elevator
            {
                Id = 1,
                Capacity = 10,
                CurrentFloor = 1,
                CurrentLoad = 0,
                Status = ElevatorStatus.Idle,
                Direction = ElevatorDirection.Idle
            },
            new Elevator
            {
                Id = 2,
                Capacity = 10,
                CurrentFloor = 3,
                CurrentLoad = 0,
                Status = ElevatorStatus.Idle,
                Direction = ElevatorDirection.Idle
            },
            new Elevator
            {
                Id = 3,
                Capacity = 10,
                CurrentFloor = 5,
                CurrentLoad = 0,
                Status = ElevatorStatus.Idle,
                Direction = ElevatorDirection.Idle
            },
            new Elevator
            {
                Id = 4,
                Capacity = 10,
                CurrentFloor = 2,
                CurrentLoad = 0,
                Status = ElevatorStatus.Idle,
                Direction = ElevatorDirection.Idle }

        };

        var elevator1 = new Elevator
        {
            Id = 1,
            Capacity = 10,
            CurrentFloor = 1,
            CurrentLoad = 0,
            Status = ElevatorStatus.Idle,
            Direction = ElevatorDirection.Idle
        };
        var elevator2 = new Elevator
        {
            Id = 2,
            Capacity = 10,
            CurrentFloor = 1,
            CurrentLoad = 0,
            Status = ElevatorStatus.Idle,
            Direction = ElevatorDirection.Idle
        };
        var elevator3 = new Elevator
        {
            Id = 3,
            Capacity = 10,
            CurrentFloor = 1,
            CurrentLoad = 0,
            Status = ElevatorStatus.Idle,
            Direction = ElevatorDirection.Idle
        };
        var elevator4 = new Elevator
        {
            Id = 4,
            Capacity = 10,
            CurrentFloor = 1,
            CurrentLoad = 0,
            Status = ElevatorStatus.Idle,
            Direction = ElevatorDirection.Idle
        };

        _elevators.TryAdd(elevator1.Id, elevator1);
        _elevators.TryAdd(elevator2.Id, elevator2);
        _elevators.TryAdd(elevator3.Id, elevator3);
        _elevators.TryAdd(elevator4.Id, elevator4);

    }

    public async Task<Elevator> CreateAsync(Elevator entity)
    {
        if (!_elevators.TryAdd(entity.Id, entity))
        {
            throw new CreatingDuplicateException("Elevator with the same ID already exists.");
        }
        return entity;
    }

    public async Task<Elevator> CreateToListAsync(Elevator entity)
    {
        entity.Id = IdGenerator.GetElevatorNextId(); // Generate a new ID
        _elevatorList.Add(entity);
        return entity;
    }

    public Task<Elevator> DeleteAsync(Elevator entity)
    {
        var elevator = _elevators.Values.FirstOrDefault(e => e.Id == entity.Id);
        if (elevator == null)
        {
            throw new NotFoundException("Elevator with the ID was not found.");
        }
        _elevators.TryRemove(entity.Id, out _);
        return Task.FromResult(entity);
    }

    public async Task<Elevator> DeleteFromListAsync(Elevator entity)
    {
        _elevators.TryGetValue(entity.Id, out var elevator);
        if (elevator == null)
        {
            throw new NotFoundException("Elevator with the ID was not found.");
        }

        _elevatorList.Remove(elevator);
        return elevator;
    }

    public IQueryable<Elevator> FindAll()
    {
        return _elevators.Values.AsQueryable();
    }

    public IQueryable<Elevator> FindAllFromList()
    {
        return _elevatorList.AsQueryable();
    }

    public IQueryable<Elevator> FindByCondition(Expression<Func<Elevator, bool>> expression)
    {
        return _elevators.Values.AsQueryable().Where(expression);
    }

    public IQueryable<Elevator> FindByConditionFromList(Expression<Func<Elevator, bool>> expression)
    {
        return _elevatorList.AsQueryable().Where(expression);
    }

    public async Task<Elevator?> FindByIdAsync(int id)
    {
        _elevators.TryGetValue(id, out var elevator);
        return elevator;
    }

    public async Task<Elevator?> FindByIdFromListAsync(int id)
    {
        var elevator = _elevatorList.FirstOrDefault(e => e.Id == id); 
        return elevator;
    }

    public async Task<Elevator> UpdateAsync(Elevator entity)
    {
        if (!_elevators.ContainsKey(entity.Id))
        {
            throw new KeyNotFoundException("Elevator not found.");
        }

        _elevators[entity.Id] = entity; // Overwrite the existing elevator
        return entity;
    }

    public async Task<Elevator> UpdateToListAsync(Elevator entity)
    {
        var elevator = _elevatorList.FirstOrDefault(e => e.Id == entity.Id);
        if (elevator != null)
        {
            // Update the elevator details (you can update specific fields as needed)
            elevator.Capacity = entity.Capacity;
            elevator.CurrentFloor = entity.CurrentFloor;
            elevator.CurrentLoad = entity.CurrentLoad;
            elevator.Status = entity.Status;
            elevator.Direction = entity.Direction;
            elevator.RequestQueue = entity.RequestQueue;
        }

        return elevator ?? throw new InvalidOperationException("Elevator not found");
    }
}
