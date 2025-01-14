using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Shared.Utilities;

public static class IdGenerator
{
    private static int _currentId;
    private static HashSet<int> _generatedIds = new HashSet<int>([1, 2, 3, 4, 5, 6, 7, 8]);

    public static int GetUniqueId()
    {
        int id;
        do
        {
            id = _currentId++;
        } while (_generatedIds.Contains(id));

        _generatedIds.Add(id);
        return id;
    }
}
