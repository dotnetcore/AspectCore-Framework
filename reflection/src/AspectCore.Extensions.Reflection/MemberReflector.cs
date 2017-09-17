using System;
using System.Reflection;
using System.Linq;

namespace AspectCore.Extensions.Reflection
{
    public abstract partial class MemberReflector<TMemberInfo> where TMemberInfo : MemberInfo
    {
        protected TMemberInfo _reflectionInfo;

        public virtual string Name => _reflectionInfo.Name;

        protected MemberReflector(TMemberInfo reflectionInfo)
        {
            _reflectionInfo = reflectionInfo ?? throw new ArgumentNullException(nameof(reflectionInfo));
            _customAttributeReflectors = _reflectionInfo.CustomAttributes.Select(data => CustomAttributeReflector.Create(data)).ToArray();
        }

        public override string ToString() => $"{_reflectionInfo.MemberType} : {_reflectionInfo}  DeclaringType : {_reflectionInfo.DeclaringType}";

        public TMemberInfo GetMemberInfo() => _reflectionInfo;

        public virtual string DisplayName => _reflectionInfo.Name;
    }
}