using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;
using System.Collections.Concurrent;

namespace AspectCore.Core.Injectors
{
    internal sealed class ServiceTable
    {
        


        public ServiceTable(IEnumerable<ServiceDefinition> serviceDefinitions)
        {
            _initialServiceDefinitions = serviceDefinitions;
        }
    }
}