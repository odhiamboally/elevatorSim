using ES.Application.Abstractions.IServices;
using ES.Application.Dtos.Common;
using ES.Application.Dtos.Elevator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Implementations.Services;


internal sealed class FloorQueueManager : IFloorQueueManager
{
    public FloorQueueManager()
    {
            
    }

    public async Task<Response<bool>> AddToQueue(int floorId, ElevatorRequest request)
    {
        try
        {
            
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<ElevatorRequest>>? DequeueRequest(int floorId)
    {
        try
        {
            
        }
        catch (Exception)
        {

            throw;
        }
    }

    public async Task<Response<Queue<ElevatorRequest>>> GetFloorQueue(int floorId)
    {
        try
        {
            
        }
        catch (Exception)
        {

            throw;
        }
    }
}
