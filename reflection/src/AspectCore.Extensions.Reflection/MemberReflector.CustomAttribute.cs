using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AspectCore.Extensions.Reflection
{
    public abstract partial class MemberReflector<TMemberInfo> : ICustomAttributeReflectorProvider where TMemberInfo : MemberInfo
    {
        private readonly CustomAttributeReflector[] _customAttributeReflectors;

        public CustomAttributeReflector[] CustomAttributeReflectors => _customAttributeReflectors;
    }
}