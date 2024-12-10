using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ES.Infrastructure.Utilities;


public class KeyedServiceResolver<TKey, TService> : IKeyedServiceResolver<TKey, TService>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDictionary<TKey, Type> _keyedServices;

    public KeyedServiceResolver(IServiceProvider serviceProvider, IDictionary<TKey, Type> keyedServices)
    {
        _serviceProvider = serviceProvider;
        _keyedServices = keyedServices;
    }

    public TService Resolve(TKey key)
    {
        if (_keyedServices.TryGetValue(key, out var implementationType))
        {
            return (TService)_serviceProvider.GetRequiredService(implementationType);
        }

        throw new KeyNotFoundException($"Service not found for key '{key}'.");
    }
}

