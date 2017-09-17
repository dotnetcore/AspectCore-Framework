using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class TypeReflector : MemberReflector<TypeInfo>
    {
        private TypeReflector(TypeInfo typeInfo) : base(typeInfo)
        {
        }
    }
}
