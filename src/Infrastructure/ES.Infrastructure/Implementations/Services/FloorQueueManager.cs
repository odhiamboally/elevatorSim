using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ES.Application.Abstractions.IServices;

namespace ES.Infrastructure.Implementations.Services;


internal sealed class FloorQueueManager : IFloorQueueManager
{
    public FloorQueueManager()
    {
        
    }

    public Task ProcessAllFloorQueues()
    {
        throw new NotImplementedException();
    }
}
