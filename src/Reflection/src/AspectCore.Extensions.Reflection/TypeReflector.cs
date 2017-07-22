using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public sealed class TypeReflector : MemberReflector<TypeInfo>
    {

        #region protected
        private TypeReflector(TypeInfo typeInfo) : base(typeInfo)
        {
        }
        #endregion

        #region internal
        internal static TypeReflector Create(TypeInfo typeInfo)
        {
            return new TypeReflector(typeInfo);
        }
        #endregion

        #region public

        public TypeInfo AsTypeInfo()
        {
            return _reflectionInfo;
        }

        #endregion

    }
}
