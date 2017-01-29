using AspectCore.Abstractions.Resolution;
using AspectCore.Abstractions.Resolution.Generators;
using AspectCore.Abstractions.Resolution.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Extensions
{
    internal static class MethodInfoConstant
    {
        internal static readonly ConstructorInfo AspectActivatorContex_Ctor = new AspectActivatorContextGenerator().CreateTypeInfo().DeclaredConstructors.Single();

        internal static readonly ConstructorInfo Object_Ctor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();

        internal static readonly MethodInfo GetAspectActivator = MethodInfosExtensions.GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

        internal static readonly MethodInfo AspectActivator_Invoke = MethodInfosExtensions.GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

        internal static readonly MethodInfo AspectActivator_InvokeAsync = MethodInfosExtensions.GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeAsync));

        internal static readonly MethodInfo ServiceInstanceProvider_GetInstance = MethodInfosExtensions.GetMethod<Func<IServiceInstanceProvider, Type, object>>((p, type) => p.GetInstance(type));
    }
}
