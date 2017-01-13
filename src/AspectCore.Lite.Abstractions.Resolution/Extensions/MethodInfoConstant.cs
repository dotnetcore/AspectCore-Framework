using AspectCore.Lite.Abstractions.Extensions;
using System;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Resolution.Extensions
{
    internal static class MethodInfoConstant
    {
        internal static readonly MethodInfo GetAspectActivator = MethodInfoHelpers.GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

        internal static readonly MethodInfo AspectActivator_Invoke = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

        internal static readonly MethodInfo AspectActivator_InvokeAsync = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeAsync));

        internal static readonly MethodInfo AspectActivator_InitializeMetaData = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.InitializeMetaData));

        internal static readonly MethodInfo SupportOriginalService_GetService = MethodInfoHelpers.GetMethod<Func<ISupportOriginalService, Type, object>>((p, type) => p.GetService(type));
    }
}
