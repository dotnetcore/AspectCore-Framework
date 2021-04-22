using System;
using System.Reflection;
using System.Linq;

namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// 成员反射操作
    /// </summary>
    /// <typeparam name="TMemberInfo">成员类型</typeparam>
    public abstract partial class MemberReflector<TMemberInfo> where TMemberInfo : MemberInfo
    {
        /// <summary>
        /// 成员
        /// </summary>
        protected TMemberInfo _reflectionInfo;

        /// <summary>
        /// 成员名称
        /// </summary>
        public virtual string Name => _reflectionInfo.Name;

        /// <summary>
        /// 成员反射操作
        /// </summary>
        /// <param name="reflectionInfo">成员</param>
        protected MemberReflector(TMemberInfo reflectionInfo)
        {
            _reflectionInfo = reflectionInfo ?? throw new ArgumentNullException(nameof(reflectionInfo));
            _customAttributeReflectors = _reflectionInfo.CustomAttributes.Select(data => CustomAttributeReflector.Create(data)).ToArray();
        }

        /// <summary>
        /// 获取字符串表示
        /// </summary>
        /// <returns>字符串表示</returns>
        public override string ToString() => $"{_reflectionInfo.MemberType} : {_reflectionInfo}  DeclaringType : {_reflectionInfo.DeclaringType}";

        /// <summary>
        /// 获取包装的MemberInfo类型的成员
        /// </summary>
        /// <returns></returns>
        public TMemberInfo GetMemberInfo() => _reflectionInfo;

        /// <summary>
        /// 显示名称
        /// </summary>
        public virtual string DisplayName => _reflectionInfo.Name;
    }
}