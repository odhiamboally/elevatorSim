using ES.Application.Abstractions.ICommands;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Entities;

namespace ES.Application.Abstractions.IServices;

public interface IElevatorService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Response<ElevatorInfo>> CompleteRequest(CompleteRequest request);

    /// <summary>
    /// Dispatches the elevator to the requested floor and initiates movement.
    /// </summary>
    /// <param name="elevatorId">The unique identifier of the elevator to be dispatched.</param>
    /// <param name="request">The request specifying the destination floor and direction.</param>
    /// <returns>Response indicating whether the dispatch was successful or not.</returns>
    Task<Response<bool>> DispatchElevator(int elevatorId, RequestInfo request);

    /// <summary>
    /// Dispatches the elevator to the requested floor and initiates movement.
    /// </summary>
    /// <param name="elevatorId">The unique identifier of the elevator to be dispatched.</param>
    /// <param name="request">The request specifying the destination floor and direction.</param>
    /// <returns>Response indicating whether the dispatch was successful or not.</returns>
    Task<Response<ElevatorInfo>> DispatchElevator(DispatchElevatorRequest request);

    /// <summary>
    /// Finds the nearest available elevator for the request.
    /// </summary>
    /// <param name="request">The elevator request containing details like the request floor, direction, etc.</param>
    /// <returns>Response containing the nearest available ElevatorInfo, or a failure response if no elevator is available.</returns>
    Task<Response<bool>> EnqueueRequestsToElevators();
    Task<Response<ElevatorInfo>> FindElevator(RequestInfo request);
    Task<Response<ElevatorInfo>> FindElevatorOptimized(RequestInfo request);

    /// <summary>
    /// Loads passengers into the elevator. Manages elevator capacity and handles partial loads if capacity is exceeded.
    /// </summary>
    /// <param name="elevatorId">The unique identifier of the elevator.</param>
    /// <param name="request">The request specifying the number of people and the destination floor.</param>
    /// <returns>Response containing updated ElevatorInfo with the new load or a failure response if loading is unsuccessful.</returns>
    Task<Response<ElevatorInfo>> LoadElevator(int elevatorId, RequestInfo request);

    Task<Response<ElevatorInfo>> LoadElevator(LoadElevatorRequest request);

    Task<Response<ElevatorInfo>> OffloadElevator(OffloadRequest request);

    /// <summary>
    /// Resets the elevator to its initial state (e.g., returning it to the ground floor and clearing any load or requests).
    /// </summary>
    /// <param name="elevatorId">The unique identifier of the elevator.</param>
    /// <returns>Response containing the reset ElevatorInfo or a failure response if reset fails.</returns>
    Task<Response<ElevatorInfo>> ResetElevator(int elevatorId, ElevatorInfo elevator);


}
