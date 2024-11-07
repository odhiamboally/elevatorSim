using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Entities;
using ES.Domain.Enums;
using ES.Shared.Exceptions;
using Microsoft.Extensions.Configuration;

namespace ES.Infrastructure.Implementations.Services;
internal sealed class ElevatorService : IElevatorService
{
    private readonly IConfiguration _config;
    

    public ElevatorService(IConfiguration config)
    {
        _config = config;
            
    }

    public Task<Response<ElevatorInfo>> DispatchElevator(ElevatorRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<Response<ElevatorInfo>> FindNearestElevator(ElevatorRequest request)
    {
        try
        {
            // Choose the closest elevator that is idle or moving towards the request's direction
            var nearestElevator = _elevators
                .Where(elevator => elevator.Status == Status.Idle || elevator.Direction == request.Direction)
                .OrderBy(elevator => Math.Abs(elevator.CurrentFloor - request.RequestedFloor))
                .FirstOrDefault();

            if (nearestElevator == null)
            {
                return await Task.FromResult(Response<ElevatorInfo>.Failure(""));
                
            }

            nearestElevator.AddToQueue(request.RequestedFloor); // Queue the floor request for the elevator

            return Task.FromResult(Response<ElevatorInfo>.Success("", nearestElevator));
        }
        catch (Exception)
        {

            throw;
        }
    }

    public Task<Response<ElevatorInfo>> LoadElevator(ElevatorRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<Response<ElevatorInfo>> OffLoadElevator(ElevatorRequest request)
    {
        throw new NotImplementedException();
    }

    public void ResetElevator(ElevatorInfo elevator)
    {
        throw new NotImplementedException();
    }

    private (bool, int) CanLoad(ElevatorRequest request)
    {
        if (request.PeopleCount >= int.Parse(_config["Elevator:Capacity"]!))
        {
            return (false, 0); ;

        }

        return (true, (int.Parse(_config["Elevator:Capacity"]!) - request.PeopleCount));
    }

    private void Load(ElevatorRequest request)
    {
        if (!CanLoad(request).Item1)
            throw new FullCapacityException("Elevator is at full capacity.");

       //Load nearest elevator that was found for the request
    }

}
