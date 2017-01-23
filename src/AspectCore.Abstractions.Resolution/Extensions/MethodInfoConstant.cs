using AspectCore.Abstractions.Resolution;
using AspectCore.Abstractions.Resolution.Generators;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Extensions
{
    internal static class MethodInfoConstant
    {
        internal static readonly ConstructorInfo AspectActivatorContex_Ctor = new AspectActivatorContextGenerator().CreateTypeInfo().DeclaredConstructors.Single();

        internal static readonly MethodInfo GetAspectActivator = MethodExtensions.GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

        internal static readonly MethodInfo AspectActivator_Invoke = MethodExtensions.GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

        internal static readonly MethodInfo AspectActivator_InvokeAsync = MethodExtensions.GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeAsync));

        internal static readonly MethodInfo TargetInstanceProvider_GetInstance = MethodExtensions.GetMethod<Func<TargetInstanceProvider, Type, object>>((p, type) => p.GetInstance(type));
    }
}
