using AspectCore.Lite.Abstractions.Common;
using System;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Resolution.Common
{
    internal static class MethodInfoConstant
    {
        internal static readonly MethodInfo GETASPECTACTIVATOR = MethodInfoHelpers.GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

        internal static readonly MethodInfo ASPECTACTIVATOR_INVOKE = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

        internal static readonly MethodInfo ASPECTACTIVATOR_INVOKEASYNC = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeAsync));

        internal static readonly MethodInfo ASPECTACTIVATOR_INITIALIZEMETADATA = MethodInfoHelpers.GetMethod<IAspectActivator>(nameof(IAspectActivator.InitializeMetaData));

        internal static readonly MethodInfo SUPPORTORIGINALSERVICE_GETSERVICE = MethodInfoHelpers.GetMethod<Func<ISupportOriginalService, Type, object>>((p, type) => p.GetService(type));
    }
}
