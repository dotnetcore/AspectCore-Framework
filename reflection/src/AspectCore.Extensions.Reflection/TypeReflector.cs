using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class TypeReflector : MemberReflector<TypeInfo>
    {
        private readonly string _displayName;
        private readonly string _fullDisplayName;

        private TypeReflector(TypeInfo typeInfo) : base(typeInfo)
        {
            _displayName = GetDisplayName(typeInfo);
            _fullDisplayName = GetFullDisplayName(typeInfo);
        }

        public override string DisplayName => _displayName;

        public virtual string FullDisplayName => _fullDisplayName;

        private static string GetDisplayName(TypeInfo typeInfo)
        {
            var name = typeInfo.Name.Replace('+', '.');
            if (typeInfo.IsGenericParameter)
            {
                return name;
            }
            if (typeInfo.IsGenericType)
            {
                var arguments = typeInfo.IsGenericTypeDefinition
                 ? typeInfo.GenericTypeParameters
                 : typeInfo.GenericTypeArguments;
                name = name.Replace("`", "").Replace(arguments.Length.ToString(), "");
                name += $"<{GetDisplayName(arguments[0].GetTypeInfo())}";
                for (var i = 1; i < arguments.Length; i++)
                {
                    name = name + "," + GetDisplayName(arguments[i].GetTypeInfo());
                }
                name += ">";
            }
            if (!typeInfo.IsNested)
                return name;
            return $"{GetDisplayName(typeInfo.DeclaringType.GetTypeInfo())}.{name}";
        }

        private static string GetFullDisplayName(TypeInfo typeInfo)
        {
            var name = typeInfo.Name.Replace('+', '.');
            if (typeInfo.IsGenericParameter)
            {
                return name;
            }
            if (!typeInfo.IsNested)
            {
                name = $"{typeInfo.Namespace}." + name;
            }
            else
            {
                name= $"{GetFullDisplayName(typeInfo.DeclaringType.GetTypeInfo())}.{name}";
            }          
            if (typeInfo.IsGenericType)
            {
                var arguments = typeInfo.IsGenericTypeDefinition
                 ? typeInfo.GenericTypeParameters
                 : typeInfo.GenericTypeArguments;
                name = name.Replace("`", "").Replace(arguments.Length.ToString(), "");
                name += $"<{GetFullDisplayName(arguments[0].GetTypeInfo())}";
                for (var i = 1; i < arguments.Length; i++)
                {
                    name += "," + GetFullDisplayName(arguments[i].GetTypeInfo());
                }
                name += ">";
            }
            return name;
        }
    }
}