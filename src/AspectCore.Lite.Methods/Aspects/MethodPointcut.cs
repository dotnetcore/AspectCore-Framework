using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace AspectCore.Lite.Methods.Aspects
{
    public class MethodPointcut : IPointcut
    {
        public bool IsMatch(MemberInfo memberInfo)
        {
            MethodInfo methodInfo = memberInfo as MethodInfo;
            if (methodInfo == null) return false;
            return IsMatch(methodInfo);
        }

        protected virtual bool IsMatch(MethodInfo methodInfo)
        {
            return true;
        }
    }
}
