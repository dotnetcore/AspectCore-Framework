using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AspectCore.Extensions.Reflection
{
    public abstract class MemberReflector<TMemberInfo> where TMemberInfo : MemberInfo
    {
        protected TMemberInfo _reflectionInfo;

        public virtual string Name => _reflectionInfo.Name;

        protected MemberReflector(TMemberInfo reflectionInfo) => _reflectionInfo = reflectionInfo ?? throw new ArgumentNullException(nameof(reflectionInfo));

        public override string ToString() => $"{_reflectionInfo.MemberType} : {_reflectionInfo}  DeclaringType : {_reflectionInfo.DeclaringType}";

        public TMemberInfo GetMemberInfo() => _reflectionInfo;
    }
}