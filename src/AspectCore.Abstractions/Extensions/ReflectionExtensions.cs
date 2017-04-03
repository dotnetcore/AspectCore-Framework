using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Abstractions.Internal;
using AspectCore.Abstractions.Internal.Generator;

namespace AspectCore.Abstractions.Extensions
{
    public static class ReflectionExtensions
    {
        #region public

        public static Type MakeDefType(this TypeInfo byRefTypeInfo)
        {
            if (byRefTypeInfo == null)
            {
                throw new ArgumentNullException(nameof(byRefTypeInfo));
            }
            if (!byRefTypeInfo.IsByRef)
            {
                throw new ArgumentException($"Type {byRefTypeInfo} is not passed by reference.");
            }

            var assemblyQualifiedName = byRefTypeInfo.AssemblyQualifiedName;
            var index = assemblyQualifiedName.IndexOf('&');
            assemblyQualifiedName = assemblyQualifiedName.Remove(index, 1);

            return byRefTypeInfo.Assembly.GetType(assemblyQualifiedName, true);
        }

        public static bool IsDynamically(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.IsDefined(typeof(DynamicallyAttribute), false);
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

            if (typeInfo.IsDefined(typeof(NonAspectAttribute)) || typeInfo.IsDynamically())
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

        public static object FastInvoke(this MethodInfo method, object instance, params object[] parameters)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return new MethodAccessor(method).CreateMethodInvoker()(instance, parameters);
        }

        public static TResult FastInvoke<TResult>(this MethodInfo method, object instance, params object[] parameters)
        {
            return (TResult)FastInvoke(method, instance, parameters);
        }

        public static Type[] GetParameterTypes(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return method.GetParameters().Select(parame => parame.ParameterType).ToArray();
        }

        public static bool IsPropertyBinding(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return method.GetBindingProperty() != null;
        }

        public static PropertyInfo GetBindingProperty(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            foreach (var property in method.DeclaringType.GetTypeInfo().DeclaredProperties)
            {
                if (property.CanRead && property.GetMethod == method)
                {
                    return property;
                }

                if (property.CanWrite && property.SetMethod == method)
                {
                    return property;
                }
            }

            return null;
        }

        public static object FastGetValue(this PropertyInfo property, object instance)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return new PropertyAccessor(property).CreatePropertyGetter()(instance);
        }

        public static TReturn FastGetValue<TReturn>(this PropertyInfo property, object instance)
        {
            return (TReturn)FastGetValue(property, instance);
        }

        public static void FastSetValue(this PropertyInfo property, object instance, object value)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            new PropertyAccessor(property).CreatePropertySetter()(instance, value);
        }

        #endregion

        #region internal

        internal static MethodInfo GetMethodBySign(this TypeInfo typeInfo, MethodInfo method)
        {
            if (method.IsGenericMethod)
            {
                foreach (var genericMethod in typeInfo.DeclaredMethods.Where(m => m.IsGenericMethod))
                {
                    if (method.ToString() == genericMethod.ToString())
                    {
                        return genericMethod;
                    }
                }
            }

            return typeInfo.GetMethod(method.Name, method.GetParameterTypes());
        }

        internal static MethodInfo GetMethod<T>(Expression<T> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }
            var methodCallExpression = expression.Body as MethodCallExpression;
            if (methodCallExpression == null)
            {
                throw new InvalidCastException("Cannot be converted to MethodCallExpression");
            }
            return methodCallExpression.Method;
        }

        internal static MethodInfo GetMethod<T>(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return typeof(T).GetTypeInfo().GetMethod(name);
        }

        internal static MethodInfo ReacquisitionIfDeclaringTypeIsGenericTypeDefinition(this MethodInfo methodInfo, Type closedGenericType)
        {
            if (!methodInfo.DeclaringType.GetTypeInfo().IsGenericTypeDefinition)
            {
                return methodInfo;
            }

            return closedGenericType.GetTypeInfo().GetMethod(methodInfo.Name, methodInfo.GetParameterTypes());
        }

        internal static bool IsCallvirt(this MethodInfo methodInfo)
        {
            var typeInfo = methodInfo.DeclaringType.GetTypeInfo();
            if (typeInfo.IsClass)
            {
                return false;
            }
            return true;
        }

        internal static string GetFullName(this MemberInfo member)
        {
            var declaringType = member.DeclaringType.GetTypeInfo();
            if (declaringType.IsInterface)
            {
                return $"{declaringType.Name}.{member.Name}".Replace('+', '.');
            }
            return member.Name;
        }

        internal static bool IsReturnTask(this MethodInfo methodInfo)
        {
            return typeof(Task).GetTypeInfo().IsAssignableFrom(methodInfo.ReturnType.GetTypeInfo());
        }

        internal static bool IsAccessibility(this PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return (property.CanRead && AspectValidator.IsAccessibility(property.GetMethod)) || (property.CanWrite && AspectValidator.IsAccessibility(property.GetMethod));
        }

        internal static class MethodInfoConstant
        {
            internal static readonly MethodInfo GetAspectActivator = GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

            internal static readonly MethodInfo AspectActivator_Invoke = GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

            internal static readonly MethodInfo AspectActivator_InvokeAsync = GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeAsync));

            internal static readonly MethodInfo ServiceInstanceProvider_GetInstance = GetMethod<Func<IServiceInstanceProvider, Type, object>>((p, type) => p.GetInstance(type));

            internal static readonly MethodInfo GetTypeFromHandle = GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

            internal static readonly MethodInfo GetMethodFromHandle = GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

            internal static readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });

            internal static readonly ConstructorInfo AspectActivatorContex_Ctor = new AspectActivatorContextGenerator().CreateTypeInfo().DeclaredConstructors.Single();

            internal static readonly ConstructorInfo Object_Ctor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();
        }

        #endregion
    }
}
