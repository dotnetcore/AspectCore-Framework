using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Utils
{
    internal static class MethodUtils
    {

        internal static readonly MethodInfo CreateAspectActivator = GetMethod<IAspectActivatorFactory>(nameof(IAspectActivatorFactory.Create));

        internal static readonly MethodInfo AspectActivatorInvoke = GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

        internal static readonly MethodInfo AspectActivatorInvokeTask = GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeTask));

        internal static readonly MethodInfo AspectActivatorInvokeValueTask = GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeValueTask));

        internal static readonly ConstructorInfo AspectActivatorContextCtor = typeof(AspectActivatorContext).GetTypeInfo().DeclaredConstructors.First();

        internal static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();

        internal static readonly MethodInfo GetParameters = typeof(AspectActivatorContext).GetTypeInfo().GetMethod("get_Parameters");

        internal static readonly MethodInfo GetMethodReflector = GetMethod<Func<MethodInfo, MethodReflector>>(m => ReflectorExtensions.GetReflector(m));

        internal static readonly MethodInfo ReflectorInvoke = GetMethod<Func<MethodReflector, object, object[], object>>((r, i, a) => r.Invoke(i, a));

        /// <summary>
        /// 获取MethodCallExpression表达式树中的方法
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>MethodInfo</returns>
        private static MethodInfo GetMethod<T>(Expression<T> expression)
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
        /// 获取类型T中名称为name的方法
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="name">方法名称</param>
        /// <returns>MethodInfo</returns>
        private static MethodInfo GetMethod<T>(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return typeof(T).GetTypeInfo().GetMethod(name);
        }
    }
}
