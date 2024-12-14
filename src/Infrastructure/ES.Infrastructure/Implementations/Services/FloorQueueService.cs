using AutoMapper;

using ES.Application.Abstractions.Interfaces;
using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Enums;
using ES.Infrastructure.Configurations.MappingProfiles;

using MassTransit.Internals;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Services;


internal sealed class FloorQueueService : IFloorQueueService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FloorQueueService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {

        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork)); 
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

    }

    public async Task<Response<RequestInfo>> AddRequest(RequestInfo request)
    {
        try
        {
            var entity = MappingProfile.MapToEntity(request);
            var createdEntity = await _unitOfWork.FloorQueueRepository.CreateAsync(entity);
            await _unitOfWork.CompleteAsync();

            return Response<RequestInfo>.Success("Request added successfully.", MappingProfile.MapToDto(createdEntity));
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Response<List<RequestInfo>>> GetAllRequests()
    {
        try
        {
            var requests = await Task.Run(() =>
                _unitOfWork.FloorQueueRepository.FindAll().ToList());

            var mappedRequests = requests.Select(MappingProfile.MapToDto).ToList();
            return Response<List<RequestInfo>>.Success("Requests retrieved successfully.", mappedRequests);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Response<List<RequestInfo>>> GetByElevatorId(RequestInfo request)
    {
        try
        {
            var requests = await Task.Run(() =>
                _unitOfWork.FloorQueueRepository
                    .FindByCondition(r => r.ElevatorId == request.ElevatorId)
                    .ToList());

            var mappedRequests = requests.Select(MappingProfile.MapToDto).ToList();
            return Response<List<RequestInfo>>.Success("Requests retrieved successfully.", mappedRequests);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Response<List<RequestInfo>>> GetByFloorId(RequestInfo request)
    {
        try
        {
            var requests = await Task.Run(() =>
                _unitOfWork.FloorQueueRepository
                    .FindByCondition(r => r.FromFloor == request.FromFloor)
                    .ToList());

            var mappedRequests = requests.Select(MappingProfile.MapToDto).ToList();
            return Response<List<RequestInfo>>.Success("Requests retrieved successfully.", mappedRequests);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Response<List<RequestInfo>>> GetByFloorIdThenByElevatorId(RequestInfo request)
    {
        try
        {
            var requests = await Task.Run(() =>
                _unitOfWork.FloorQueueRepository
                    .FindByCondition(r => r.FromFloor == request.FromFloor && r.ElevatorId == request.ElevatorId)
                    .ToList());

            var mappedRequests = requests.Select(MappingProfile.MapToDto).ToList();
            return Response<List<RequestInfo>>.Success("Requests retrieved successfully.", mappedRequests);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Response<RequestInfo>> RemoveRequest(RequestInfo request)
    {
        try
        {
            var entity = MappingProfile.MapToEntity(request);
            var deletedEntity = await _unitOfWork.FloorQueueRepository.DeleteAsync(entity);
            await _unitOfWork.CompleteAsync();

            return Response<RequestInfo>.Success("Request removed successfully.", MappingProfile.MapToDto(deletedEntity));
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Response<RequestInfo>> UpdateRequest(RequestInfo request)
    {
        try
        {
            var entity = MappingProfile.MapToEntity(request);
            var updatedEntity = await _unitOfWork.FloorQueueRepository.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync();

            return Response<RequestInfo>.Success("Request updated successfully.", MappingProfile.MapToDto(updatedEntity));
        }
        catch (Exception)
        {
            throw;
        }
    }



    #region Private Methods

    


    #endregion
}
