using AspectCore.Abstractions.Resolution;
using AspectCore.Abstractions.Resolution.Generators;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Extensions
{
    internal static class MethodInfoConstant
    {
        private static readonly TypeInfo activatorContextType = new AspectActivatorContextGenerator().CreateTypeInfo();

        internal static readonly ConstructorInfo AspectActivatorContex_Ctor = activatorContextType.DeclaredConstructors.Single();

        internal static readonly MethodInfo GetAspectActivator = MethodInfoHelpers.GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

        internal static readonly MethodInfo AspectActivator_Invoke = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

        internal static readonly MethodInfo AspectActivator_InvokeAsync = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeAsync));

        internal static readonly MethodInfo TargetInstanceProvider_GetInstance = MethodInfoHelpers.GetMethod<Func<TargetInstanceProvider, Type, object>>((p, type) => p.GetInstance(type));
    }
}
