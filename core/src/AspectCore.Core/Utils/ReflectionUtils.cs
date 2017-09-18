using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    public static class ReflectionUtils
    {
        public static bool IsProxy(this object instance)
        {
            if (instance == null)
            {
                return false;
            }
            return instance.GetType().GetTypeInfo().IsProxyType();
        }

        public static bool IsProxyType(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.GetReflector().IsDefined(typeof(DynamicallyAttribute));
        }

        public static bool CanInherited(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (!typeInfo.IsClass || typeInfo.IsSealed)
            {
                return false;
            }

            if (typeInfo.IsNonAspect() || typeInfo.IsProxyType())
            {
                return false;
            }

            if (typeInfo.IsNested)
            {
                return typeInfo.IsNestedPublic && typeInfo.DeclaringType.GetTypeInfo().IsPublic;
            }
            else
            {
                return typeInfo.IsPublic;
            }
        }

        internal static Type[] GetParameterTypes(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return method.GetParameters().Select(parame => parame.ParameterType).ToArray();
        }

        public static bool IsNonAspect(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.GetReflector().IsDefined(typeof(NonAspectAttribute));
        }

        public static bool IsNonAspect(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            return methodInfo.GetReflector().IsDefined(typeof(NonAspectAttribute));
        }

        internal static bool IsCallvirt(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            var typeInfo = methodInfo.DeclaringType.GetTypeInfo();
            if (typeInfo.IsClass)
            {
                return false;
            }
            return true;
        }

        internal static string GetDisplayName(this PropertyInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }
            var declaringType = member.DeclaringType.GetTypeInfo();
            if (declaringType.IsInterface)
            {
                return $"{declaringType.Namespace}.{declaringType.GetReflector().DisplayName}.{member.Name}";
            }
            return member.Name;
        }

        internal static string GetFullName(this MethodInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }
            var declaringType = member.DeclaringType.GetTypeInfo();
            if (declaringType.IsInterface)
            {
                return $"{declaringType.Namespace}.{declaringType.GetReflector().DisplayName}.{member.Name}";
            }
            return member.Name;
        }

        internal static string GetDisplayName(this MethodInfo member)
        {
            if (member == null)
            {
                throw new ArgumentNullException(nameof(member));
            }
            var declaringType = member.DeclaringType.GetTypeInfo();
            return
                $"{declaringType.Namespace}.{declaringType.GetReflector().DisplayName}.{member.GetReflector().DisplayName}";
        }

        //internal static string GetName(this Type type)
        //{
        //    if (!type.GetTypeInfo().IsGenericType)
        //    {
        //        return type.Name.Replace('+', '.');
        //    }
        //    var arguments = type.GetTypeInfo().IsGenericTypeDefinition
        //        ? type.GetTypeInfo().GenericTypeParameters
        //        : type.GenericTypeArguments;
        //    var name = $"{type.Name.Replace('+', '.')}<{arguments[0].GetName()}";
        //    for (var i = 1; i < arguments.Length; i++)
        //    {
        //        name = name + "," + arguments[i].GetName();
        //    }
        //    return name + ">";
        //}

        internal static bool IsReturnTask(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            return typeof(Task).GetTypeInfo().IsAssignableFrom(methodInfo.ReturnType.GetTypeInfo());
        }

        internal static bool IsReturnValueTask(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            var returnType = methodInfo.ReturnType.GetTypeInfo();
            return returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(ValueTask<>);
        }

        internal static bool IsAccessibility(this PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return (property.CanRead && property.GetMethod.IsAccessibility()) ||
                   (property.CanWrite && property.GetMethod.IsAccessibility());
        }

        internal static bool IsAccessibility(this TypeInfo declaringType)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException(nameof(declaringType));
            }
            return !(declaringType.IsNotPublic || declaringType.IsValueType || declaringType.IsSealed);
        }

        internal static bool IsAccessibility(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return !method.IsStatic && !method.IsFinal && method.IsVirtual &&
                   (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }

        internal static MethodInfo GetExplicitMethod(this TypeInfo typeInfo, MethodInfo method)
        {
            var interfaceType = method.DeclaringType;
            var explicitMethodName =
                $"{interfaceType.Namespace}.{interfaceType.GetReflector().DisplayName}.{method.Name}";
            foreach (var m in typeInfo.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (m.Name != explicitMethodName)
                {
                    continue;
                }
                var parameters1 = method.GetParameterTypes();
                var parameters2 = m.GetParameterTypes();
                if (parameters1.Length != parameters2.Length)
                {
                    continue;
                }
                if (method.GetGenericArguments().Length != method.GetGenericArguments().Length)
                {
                    continue;
                }
                for (var i = 0; i < parameters2.Length; i++)
                {
                    var p1 = parameters1[i];
                    var p2 = parameters2[i];
                    if (p1.IsGenericParameter && !p2.IsGenericParameter)
                    {
                        continue;
                    }
                    else if (!p1.IsGenericParameter && p2.IsGenericParameter)
                    {
                        continue;
                    }
                    else if (p1.IsGenericParameter && p2.IsGenericParameter)
                    {
                        return m;
                    }
                    else
                    {
                        var pt1 = p1.GetTypeInfo();
                        var pt2 = p2.GetTypeInfo();
                        if (pt1.IsGenericType && !pt2.IsGenericType)
                        {
                            continue;
                        }
                        else if (!pt1.IsGenericType && pt2.IsGenericType)
                        {
                            continue;
                        }
                        else if (pt1.IsGenericType && pt2.IsGenericType)
                        {
                            if (pt1.AsType().IsConstructedGenericType && pt2.AsType().IsConstructedGenericType &&
                                pt1 == pt2)
                            {
                                return m;
                            }
                            if (pt1.GetGenericTypeDefinition() == pt2.GetGenericTypeDefinition() &&
                                pt1.GenericTypeArguments.Length == pt2.GenericTypeArguments.Length)
                            {
                                return m;
                            }
                        }
                        else
                        {
                            if (pt1 == pt2)
                            {
                                return m;
                            }
                        }
                    }
                }
            }

            return null;
        }  
    }
}