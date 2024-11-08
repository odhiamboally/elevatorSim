using ES.Application.Abstractions.ICommands;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Entities;

namespace ES.Application.Abstractions.IServices;

public interface IElevatorService
{
    Task<Response<ElevatorInfo>> FindNearestElevator(ElevatorRequest request);
    Task<Response<ElevatorInfo>> LoadElevator(int elevatorId, ElevatorRequest request);
    Task<Response<bool>> DispatchElevator(ElevatorRequest request);
    Task<Response<ElevatorInfo>> OffLoadElevator(int elevatorId, ElevatorRequest request);
    Task<Response<ElevatorInfo>> ResetElevator(int elevatorId, ElevatorInfo elevator);
    Task UpdateElevatorQueue(ElevatorInfo elevator);
    Task CheckAndProcessPassengerQueues();




}
