using AutoMapper;

using ES.Application.Abstractions.Hubs;
using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
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

    private readonly IElevatorHub _elevatorHub;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ElevatorStateManager(IElevatorHub elevatorHub, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _elevatorHub = elevatorHub;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Response<bool>> BroadcastStateAsync(int elevatorId, ElevatorInfo updatedInfo)
    {
        try
        {
            await _elevatorHub.BroadcastElevatorStateAsync(elevatorId, updatedInfo);
            return Response<bool>.Success("Broadcast successful.", true);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<List<ElevatorInfo>>> GetAllElevatorStatesAsync()
    {
        try
        {
            var elevatorStatesResponse = await _unitOfWork.ElevatorRepository.FindAll().ToListAsync();
            var elevatorStates = _mapper.Map<List<ElevatorInfo>>(elevatorStatesResponse);
            return Response<List<ElevatorInfo>>.Success("Elevator States:", elevatorStates);

        }
        catch (Exception)
        {

            throw;
        }
    }
}
