using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Resolution;
using System;
using System.Reflection;

namespace AspectCore.Abstractions.Extensions
{
    internal static class MethodInfoConstant
    {
        internal static readonly MethodInfo GetAspectActivator = MethodInfoHelpers.GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

        internal static readonly MethodInfo AspectActivator_Invoke = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

        internal static readonly MethodInfo AspectActivator_InvokeAsync = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeAsync));

        internal static readonly MethodInfo AspectActivator_InitializeMetaData = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.InitializeMetaData));

        internal static readonly MethodInfo SupportOriginalService_GetService = MethodInfoHelpers.GetMethod<Func<IOriginalServiceProvider, Type, object>>((p, type) => p.GetService(type));
    }
}
