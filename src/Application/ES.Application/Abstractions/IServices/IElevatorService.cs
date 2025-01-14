﻿using ES.Application.Abstractions.ICommands;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using ES.Domain.Entities;

namespace ES.Application.Abstractions.IServices;

public interface IElevatorService
{

    
    /// <summary>
    /// Periodically checks and processes all passenger queues across floors and assigns elevators as needed.
    /// </summary>
    Task CheckAndProcessPassengerQueues();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<Response<bool>> CompleteRequest(CompleteRequest request);

    /// <summary>
    /// Dispatches the elevator to the requested floor and initiates movement.
    /// </summary>
    /// <param name="elevatorId">The unique identifier of the elevator to be dispatched.</param>
    /// <param name="request">The request specifying the destination floor and direction.</param>
    /// <returns>Response indicating whether the dispatch was successful or not.</returns>
    Task<Response<bool>> DispatchElevator(int elevatorId, ElevatorRequest request);

    /// <summary>
    /// Dispatches the elevator to the requested floor and initiates movement.
    /// </summary>
    /// <param name="elevatorId">The unique identifier of the elevator to be dispatched.</param>
    /// <param name="request">The request specifying the destination floor and direction.</param>
    /// <returns>Response indicating whether the dispatch was successful or not.</returns>
    Task<Response<bool>> DispatchElevator(DispatchElevatorRequest request);

    /// <summary>
    /// Finds the nearest available elevator for the request.
    /// </summary>
    /// <param name="request">The elevator request containing details like the request floor, direction, etc.</param>
    /// <returns>Response containing the nearest available ElevatorInfo, or a failure response if no elevator is available.</returns>
    Task<Response<ElevatorInfo>> FindNearestElevator(ElevatorRequest request);

    /// <summary>
    /// Handles cases where only a portion of the requested load can be accommodated (e.g., capacity constraints).
    /// Adds remaining passengers back to the floor queue as a partial request.
    /// </summary>
    /// <param name="elevatorId">The unique identifier of the elevator.</param>
    /// <param name="request">The partial load request that couldn't be fully accommodated.</param>
    /// <returns>Response containing the updated ElevatorInfo with any remaining load details.</returns>
    Task<Response<ElevatorInfo>> HandlePartialLoad(Elevator elevator, ElevatorRequest request, int availableSpace);

    /// <summary>
    /// Loads passengers into the elevator. Manages elevator capacity and handles partial loads if capacity is exceeded.
    /// </summary>
    /// <param name="elevatorId">The unique identifier of the elevator.</param>
    /// <param name="request">The request specifying the number of people and the destination floor.</param>
    /// <returns>Response containing updated ElevatorInfo with the new load or a failure response if loading is unsuccessful.</returns>
    Task<Response<ElevatorInfo>> LoadElevator(int elevatorId, ElevatorRequest request);
   
    /// <summary>
    /// Offloads passengers from the elevator at the specified floor.
    /// </summary>
    /// <param name="elevatorId">The unique identifier of the elevator.</param>
    /// <param name="request">The request specifying the number of people to offload.</param>
    /// <returns>Response containing the updated ElevatorInfo after offloading, or a failure response if offloading fails.</returns>
    Task<Response<ElevatorInfo>> OffLoadElevator(int elevatorId, ElevatorRequest request);

    /// <summary>
    /// Resets the elevator to its initial state (e.g., returning it to the ground floor and clearing any load or requests).
    /// </summary>
    /// <param name="elevatorId">The unique identifier of the elevator.</param>
    /// <returns>Response containing the reset ElevatorInfo or a failure response if reset fails.</returns>
    Task<Response<ElevatorInfo>> ResetElevator(int elevatorId, ElevatorInfo elevator);


}
