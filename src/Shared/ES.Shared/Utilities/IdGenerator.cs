using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Shared.Utilities;

public static class IdGenerator
{
    private static int _currentId = 0;

    public static int GetElevatorNextId()
    {
        return Interlocked.Increment(ref _currentId);
    }

    public static int GetRequestNextId()
    {
        return Interlocked.Increment(ref _currentId);
    }
}
