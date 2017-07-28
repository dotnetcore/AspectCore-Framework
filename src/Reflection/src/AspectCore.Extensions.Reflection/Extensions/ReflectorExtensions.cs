using AspectCore.Extensions.Reflection;

namespace System.Reflection
{
    public static class ReflectorExtensions
    {
        #region Reflection
        public static TypeReflector AsReflector(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return type.GetTypeInfo().AsReflector();
        }

        public static TypeReflector AsReflector(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return TypeReflector.Create(typeInfo);
        }

        public static ConstructorReflector AsReflector(this ConstructorInfo constructor)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException(nameof(constructor));
            }
            return ConstructorReflector.Create(constructor);
        }

        public static FieldReflector AsReflector(this FieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }
            return FieldReflector.Create(field);
        }

        public static MethodReflector AsReflector(this MethodInfo method)
        {
            return AsReflector(method, CallOptions.Callvirt);
        }

        public static MethodReflector AsReflector(this MethodInfo method, CallOptions callOption)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return MethodReflector.Create(method, callOption);
        }

        public static PropertyReflector AsReflector(this PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return AsReflector(property, CallOptions.Callvirt);
        }

        public static PropertyReflector AsReflector(this PropertyInfo property, CallOptions callOption)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return PropertyReflector.Create(property, callOption);
        }
        #endregion

        #region Reflectr

        public static FieldInfo AsFieldInfo(this FieldReflector reflector) => reflector?.AsMemberInfo();

        public static MethodInfo AsMethodInfo(this MethodReflector reflector)=> reflector?.AsMemberInfo();

        public static ConstructorInfo AsConstructorInfo(this ConstructorReflector reflector) => reflector?.AsMemberInfo();

        public static PropertyInfo AsPropertyInfo(this PropertyReflector reflector) => reflector?.AsMemberInfo();

        #endregion
    }
}
