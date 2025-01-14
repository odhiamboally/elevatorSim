using AutoMapper;

using Confluent.Kafka;

using ES.Application.Abstractions.Hubs;
using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Entities;
using ES.Domain.Enums;
using ES.Infrastructure.Implementations.Hubs;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Services;

internal sealed class ElevatorStateManager : IElevatorStateManager
{

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    private readonly IHubContext<ElevatorHub> _hubContext;

    public ElevatorStateManager(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<ElevatorHub> hubContext)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    public async Task<Response<ElevatorInfo>> FetchElevatorStateAsync(int elevatorId, ElevatorInfo updatedInfo)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveElevatorState", elevatorId, updatedInfo);
            return Response<ElevatorInfo>.Success("Broadcast successful.", updatedInfo);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<List<ElevatorInfo>>> FetchElevatorStatesAsync()
    {
        try
        {
            var elevatorStatesResponse = _unitOfWork.ElevatorRepository.FindAll().ToList();
            var elevatorStates = elevatorStatesResponse.Select(e => new ElevatorInfo(
                e.Id, 
                e.CurrentFloor, 
                e.CurrentLoad, 
                e.Status, 
                e.Direction
                ))
                .ToList();

            await _hubContext.Clients.All.SendAsync("ReceiveElevatorStates", elevatorStates);
            return Response<List<ElevatorInfo>>.Success("Elevator States:", elevatorStates);

        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<ElevatorInfo>> UpdateElevatorStateAsync(ElevatorInfo updatedInfo)
    {
        try
        {
            var elevator = new Elevator
            {
                Id = updatedInfo.Id,
                Capacity = updatedInfo.Capacity,
                CurrentFloor = updatedInfo.CurrentFloor,
                CurrentLoad = updatedInfo.CurrentLoad,
                Status = updatedInfo.Status,
                Direction = updatedInfo.Direction,
                RequestQueue = new Queue<int>(updatedInfo.RequestQueue.Select(x => x.Id))

            };

            await _unitOfWork.ElevatorRepository.UpdateAsync(elevator);
            await _hubContext.Clients.All.SendAsync("ReceiveElevatorState", elevator.Id, updatedInfo);
            return Response<ElevatorInfo>.Success("Broadcast successful.", updatedInfo);
        }
        catch (Exception)
        {

            throw;
        }        
    }

    public Task<Response<ElevatorInfo>> UpdateElevatorStatesAsync(List<ElevatorInfo> elevatorInfos)
    {
        throw new NotImplementedException();
    }
}
