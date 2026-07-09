using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;

namespace AspectCore.DynamicProxy.ProxyBuilder.Builders
{
    internal static class MethodBodyFactory
    {
        private const string TargetFieldName = "_implementation";

        public static MethodBodyNode DecideBody(
            MethodInfo serviceMethod,
            MethodInfo implementationMethod,
            MethodInfo predicateMethod,
            IAspectValidator validator,
            Type serviceType)
        {
            if (serviceMethod.IsNonAspect())
                return BuildDelegationBody(serviceMethod, implementationMethod, serviceType);

            if (validator.Validate(serviceMethod, true) || validator.Validate(implementationMethod, false))
                return BuildAspectActivatorBody(serviceMethod, implementationMethod, predicateMethod);

            return BuildDelegationBody(serviceMethod, implementationMethod, serviceType);
        }

        public static MethodBodyNode BuildDelegationBody(
            MethodInfo serviceMethod,
            MethodInfo implementationMethod,
            Type serviceType)
        {
            if (serviceType.GetTypeInfo().IsInterface || !implementationMethod.IsExplicit())
            {
                return new DirectDelegationBody(
                    TargetFieldName,
                    implementationMethod.IsExplicit() ? serviceMethod : implementationMethod,
                    serviceMethod,
                    implementationMethod.IsCallvirt(),
                    serviceType);
            }

            return new ReflectorDelegationBody(implementationMethod, serviceMethod);
        }

        public static AspectActivatorBody BuildAspectActivatorBody(
            MethodInfo serviceMethod,
            MethodInfo implementationMethod,
            MethodInfo predicateMethod)
        {
            return new AspectActivatorBody(
                serviceMethod,
                implementationMethod,
                predicateMethod,
                serviceMethod.IsGenericMethodDefinition,
                DetermineReturnKind(serviceMethod),
                serviceMethod.ReturnType);
        }

        public static StubBody BuildStubBody(MethodInfo method)
        {
            return new StubBody(method.ReturnType);
        }

        public static ReturnKind DetermineReturnKind(MethodInfo method)
        {
            if (method.ReturnType == typeof(void))
                return ReturnKind.Void;
            if (method.ReturnType == typeof(Task))
                return ReturnKind.Task;
            if (method.IsReturnTask())
                return ReturnKind.TaskOfT;
            if (method.ReturnType == typeof(ValueTask))
                return ReturnKind.ValueTask;
            if (method.IsReturnValueTask())
                return ReturnKind.ValueTaskOfT;
            return ReturnKind.Sync;
        }
    }
}
