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
        public MethodInfo TargetMethod { get; }
        public MethodInfo ProxyMethod { get; }
        public ParameterCollection Parameters { get; }
        public ParameterDescriptor ReturnParameter { get; }
    }
}