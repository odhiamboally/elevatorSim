using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Entities;

namespace ES.Application.Abstractions.IServices;

public interface IElevatorService
{
    Task<Response<ElevatorInfo>> FindNearestElevator(ElevatorRequest request);
    Task<Response<ElevatorInfo>> LoadElevator(ElevatorRequest request);
    Task<Response<ElevatorInfo>> DispatchElevator(ElevatorRequest request);
    Task<Response<ElevatorInfo>> OffLoadElevator(ElevatorRequest request);
    void ResetElevator(ElevatorInfo elevator);

}
