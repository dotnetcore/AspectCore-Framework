using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Core
{
    internal class TargetDescriptor: ITargetDescriptor
    {
        private static readonly ConcurrentDictionary<MethodInfo, MethodReflector> dictionary = new ConcurrentDictionary<MethodInfo, MethodReflector>();
        private readonly object _implementationInstance;
        private readonly MethodInfo _implementationMethod;

        public virtual MethodInfo ServiceMethod { get; }

        public virtual Type ServiceType { get; }

        public TargetDescriptor(object implementationInstance,
            MethodInfo serviceMethod, Type serviceType, MethodInfo implementationMethod)
        { 
            ServiceType = serviceType;
            ServiceMethod = serviceMethod;      
            _implementationInstance = implementationInstance;  
            _implementationMethod = implementationMethod;
        }

        public virtual object Invoke(IParameterCollection parameterCollection)
        {
            try
            {
                var reflector = dictionary.GetOrAdd(_implementationMethod, method => method.GetReflector(CallOptions.Call));
                if (parameterCollection.Count == 0)
                {
                    return reflector.Invoke(_implementationInstance);
                }
                return reflector.Invoke(_implementationInstance, parameterCollection.Select(x => x.Value).ToArray());
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}
