using AspectCore.Lite.Abstractions;
using AspectCore.Lite.DynamicProxy.Resolution;
using System;
using System.Reflection;

namespace AspectCore.Lite.DynamicProxy.Resolution.Common
{
    internal static class MethodConstant
    {
        internal static readonly MethodInfo GetAspectActivator = MethodHelper.GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

        internal static readonly MethodInfo AspectActivator_Invoke = MethodHelper.GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

        internal static readonly MethodInfo AspectActivator_InvokeAsync = MethodHelper.GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeAsync));

        internal static readonly MethodInfo AspectActivator_InitializeMetaData = MethodHelper.GetMethod<IAspectActivator>(nameof(IAspectActivator.InitializeMetaData));

        internal static readonly MethodInfo GetTypeFromHandle = MethodHelper.GetMethod<Type>(nameof(Type.GetTypeFromHandle));

        internal static readonly MethodInfo GetMothodFromHandle = MethodHelper.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

        internal static readonly MethodInfo SupportOriginalService_GetService = MethodHelper.GetMethod<Func<ISupportOriginalService, Type, object>>((p, type) => p.GetService(type));
    }
}
