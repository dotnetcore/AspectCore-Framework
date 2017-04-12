using System;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal.Generator;

namespace AspectCore.Core.Internal
{
    internal static class MethodInfoConstant
    {
        internal static readonly MethodInfo GetAspectActivator = ReflectionExtensions.GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

        internal static readonly MethodInfo AspectActivatorInvoke = ReflectionExtensions.GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

        internal static readonly MethodInfo AspectActivatorInvokeAsync = ReflectionExtensions.GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeAsync));

        internal static readonly MethodInfo ServiceInstanceProviderGetInstance = ReflectionExtensions.GetMethod<Func<IServiceInstanceProvider, Type, object>>((p, type) => p.GetInstance(type));

        internal static readonly MethodInfo GetTypeFromHandle = ReflectionExtensions.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

        internal static readonly MethodInfo GetMethodFromHandle = ReflectionExtensions.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

        internal static readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });

        internal static readonly ConstructorInfo AspectActivatorContexCtor = new AspectActivatorContextGenerator().CreateTypeInfo().DeclaredConstructors.Single();

        internal static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();
    }
}
