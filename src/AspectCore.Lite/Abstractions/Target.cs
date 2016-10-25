using AspectCore.Lite.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public sealed class Target : IMethodInvoker
    {
        internal ParameterCollection ParameterCollection { get; set; }
        public MethodInfo Method { get; }
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public object Instance { get; }

        internal Target(MethodInfo method, Type serviceType, Type implementationType, object implementationInstance)
        {
            ExceptionHelper.ThrowArgumentNull(method , nameof(method));
            ExceptionHelper.ThrowArgumentNull(serviceType , nameof(serviceType));
            ExceptionHelper.ThrowArgumentNull(implementationType , nameof(implementationType));
            ExceptionHelper.ThrowArgumentNull(implementationInstance , nameof(implementationInstance));

            Method = method;
            ServiceType = serviceType;
            ImplementationType = implementationType;
            Instance = implementationInstance;
        }

        public object Invoke()
        {
            try
            {
                object[] args = ParameterCollection?.Select(p => p.Value)?.ToArray();
                return Method.Invoke(Instance , args);
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}