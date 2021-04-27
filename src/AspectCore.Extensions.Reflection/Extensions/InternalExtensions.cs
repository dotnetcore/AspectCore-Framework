using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq.Expressions;

namespace AspectCore.Extensions.Reflection
{
    internal static class InternalExtensions
    {
        /// <summary>
        /// 比较typeInfo中声明的方法，并返回第一个与method的字符串表示相同的方法
        /// </summary>
        /// <param name="typeInfo">类型</param>
        /// <param name="method">方法</param>
        /// <returns>类型中找到的方法</returns>
        internal static MethodInfo GetMethodBySign(this TypeInfo typeInfo, MethodInfo method)
        {
            return typeInfo.DeclaredMethods.FirstOrDefault(m => m.ToString() == method.ToString());
        }

        /// <summary>
        /// 从表达式树expression中获取要调用的方法
        /// </summary>
        /// <typeparam name="T">表达式树的类型参数</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>方法</returns>
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

        /// <summary>
        /// 从类型T中获取名称为name的方法
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="name">方法名称</param>
        /// <returns>方法对象</returns>
        internal static MethodInfo GetMethod<T>(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return typeof(T).GetTypeInfo().GetMethod(name);
        }

        /// <summary>
        /// 如果声明方法的类型为类，则返回false,否则返回true(接口)
        /// </summary>
        /// <param name="methodInfo">方法</param>
        /// <returns>声明方法的类型为类，则返回false,否则返回true(接口)</returns>
        internal static bool IsCallvirt(this MethodInfo methodInfo)
        {
            var typeInfo = methodInfo.DeclaringType.GetTypeInfo();
            if (typeInfo.IsClass)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取成员的全名称
        /// </summary>
        /// <example>
        /// 如 Person.GetName
        /// </example>
        /// <param name="member">成员</param>
        /// <returns>成员的全名称</returns>
        internal static string GetFullName(this MemberInfo member)
        {
            var declaringType = member.DeclaringType.GetTypeInfo();
            if (declaringType.IsInterface)
            {
                return $"{declaringType.Name}.{member.Name}".Replace('+', '.');
            }
            return member.Name;
        }

        /// <summary>
        /// 判断方法返回值是否派生自Task类型
        /// </summary>
        /// <param name="methodInfo">待判断的方法</param>
        /// <returns>方法返回值是否派生自Task类型。true:是，false:不是</returns>
        internal static bool IsReturnTask(this MethodInfo methodInfo)
        {
            return typeof(Task).GetTypeInfo().IsAssignableFrom(methodInfo.ReturnType.GetTypeInfo());
        }

        /// <summary>
        /// 获取方法的参数类型
        /// </summary>
        /// <param name="method">方法</param>
        /// <returns>参数类型数组</returns>
        internal static Type[] GetParameterTypes(this MethodBase method)
        {
            return method.GetParameters().Select(x => x.ParameterType).ToArray();
        }

        /// <summary>
        /// 如果类型可枚举,则获取枚举项的类型
        /// </summary>
        /// <param name="typeInfo">待判断的类型</param>
        /// <returns>元素项的类型</returns>
        internal static Type UnWrapArrayType(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            if (!typeInfo.IsArray)
            {
                return typeInfo.AsType();
            }
            //typeInfo.ImplementedInterfaces: 获取当前类型实现的接口的集合
            return typeInfo.ImplementedInterfaces.First(x => x.GetTypeInfo().IsGenericType && x.GetTypeInfo().GetGenericTypeDefinition() == typeof(IEnumerable<>)).GenericTypeArguments[0];
        }
    }
}