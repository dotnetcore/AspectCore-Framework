using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal.Generator;

namespace AspectCore.Core.Internal
{
    internal static class MethodInfoConstant
    {
        internal static readonly MethodInfo GetAspectActivator = GetMethod<Func<IServiceProvider, IAspectActivator>>(provider => provider.GetAspectActivator());

        internal static readonly MethodInfo AspectActivatorInvoke = GetMethod<IAspectActivator>(nameof(IAspectActivator.Invoke));

        internal static readonly MethodInfo AspectActivatorInvokeTask = GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeTask));

        internal static readonly MethodInfo AspectActivatorInvokeValueTask = GetMethod<IAspectActivator>(nameof(IAspectActivator.InvokeValueTask));

        internal static readonly MethodInfo ServiceInstanceProviderGetInstance = GetMethod<Func<IServiceInstanceProvider, Type, object>>((p, type) => p.GetInstance(type));

        internal static readonly MethodInfo GetTypeFromHandle = GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

        internal static readonly MethodInfo GetMethodFromHandle = GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

        internal static readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });

        internal static readonly ConstructorInfo AspectActivatorContexCtor = typeof(AspectActivatorContext).GetTypeInfo().DeclaredConstructors.First(x => x.GetParameters().Length == 7);

        internal static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();

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
    }
}
