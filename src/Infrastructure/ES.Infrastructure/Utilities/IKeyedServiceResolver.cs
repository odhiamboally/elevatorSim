using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Utilities;

public interface IKeyedServiceResolver<TKey, TService>
{
    TService Resolve(TKey key);
}

