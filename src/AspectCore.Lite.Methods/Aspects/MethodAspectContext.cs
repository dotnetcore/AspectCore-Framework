using AspectCore.Lite.Abstractions.Aspects;
using AspectCore.Lite.Abstractions.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Methods.Aspects
{
    public sealed class MethodAspectContext : AspectContext
    {
        public ParameterCollection Parameters { get; }

        public ParameterDescriptor ReturnParameter { get; }

        public MethodAspectContext(ITarget target , IProxy proxy , ParameterCollection parameters , ParameterDescriptor returnParameter)
            : base(target , proxy)
        {
            Parameters = parameters;
            ReturnParameter = returnParameter;
        }

        private MethodInfo targetMethod;

        private MethodInfo proxyMethod;

        public MethodInfo TargetMethod
        {
            get
            {
                return targetMethod ??
                    CastMethodInfo()(() => Target.GetTargetMethodInfo())("Unable to resolve target method.")(targetMethod);
            }
        }

        public MethodInfo ProxyMethod
        {
            get
            {
                return proxyMethod ??
                    CastMethodInfo()(() => Proxy.GetProxyMethodInfo())("Unable to resolve proxy method.")(proxyMethod);
            }
        }

        private Func<Func<MemberInfo> , Func<string , Func<MethodInfo , MethodInfo>>> CastMethodInfo()
        {
            return memberInfoFactory =>
            {
                return exceptionMessage =>
                {
                    return methodInfo =>
                    {
                        if (methodInfo == null)
                        {
                            methodInfo = memberInfoFactory() as MethodInfo;
                            if (methodInfo == null) throw new InvalidCastException(exceptionMessage);
                        }
                        return methodInfo;
                    };
                };
            };
        }
    }
}