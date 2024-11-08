using ES.Application.Abstractions.ICommands;
using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Entities;
using ES.Domain.Enums;
using ES.Infrastructure.Implementations.Commands;
using ES.Shared.Exceptions;
using Microsoft.Extensions.Configuration;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ES.Infrastructure.Implementations.Services;
internal sealed class ElevatorService : IElevatorService
{
    private readonly IConfiguration _config;

    public ElevatorService(IConfiguration config)
    {
        _config = config;

    }

    public async Task<Response<bool>> DispatchElevator(ElevatorRequest request)
    {
        try
        {
            IElevatorCommand command = null!;
            switch (request.Direction)
            {
                case Direction.Up:
                    command = new MoveUpCommand();
                    await Dispatch(request, command);

                    break;

                case Direction.Down:
                    command = new MoveDownCommand();
                    await Dispatch(request, command);

                    break;

                case Direction.Idle:
                    command = new ResetCommand();
                    await Dispatch(request, command);

                    break;

                default:
                    command = new ResetCommand();
                    await Dispatch(request, command);

                    break;

            }

            return Response<bool>.Success("", true);
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<ElevatorInfo>> FindNearestElevator(ElevatorRequest request)
    {
        try
        {
            // Choose the closest elevator that is idle or moving towards the request's direction and can load

            // I think we need to get elevators from where they are maintained.

            var nearestElevator = _elevators
                .Where(elevator => elevator.Status == Status.Idle || elevator.Direction == request.Direction)
                .OrderBy(elevator => Math.Abs(elevator.CurrentFloor - request.RequestedFloor))
                .FirstOrDefault();

            if (nearestElevator == null)
            {
                Response<ElevatorInfo>.Failure("");

            }

            // Queue the floor request for the elevator



            //Dispatch Elevator
            await DispatchElevator(request);


            return Response<ElevatorInfo>.Success("", nearestElevator);
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

    public Task<Response<ElevatorInfo>> ResetElevator(ElevatorInfo elevator)
    {
        throw new NotImplementedException();
    }

    private async Task<bool> Dispatch(ElevatorRequest request, IElevatorCommand command) 
    {
        try
        {
            await command.ExecuteAsync();
            return true;
        }
        catch (Exception)
        {

            throw;
        }
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
