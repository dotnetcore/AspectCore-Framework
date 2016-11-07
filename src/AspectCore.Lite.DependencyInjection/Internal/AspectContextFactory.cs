using System;
using System.Linq.Expressions;
using System.Reflection;
using AspectCore.Lite.Abstractions;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal class AspectContextFactory : IAspectContextFactory
    {
        private static readonly Assembly liteAssembly = typeof(IAspectContextFactory).GetTypeInfo().Assembly;
        private static readonly Type aspectContextType = liteAssembly.GetType("AspectCore.Lite.Internal.AspectContext");

        private static readonly ConstructorInfo constructorInfo =
            aspectContextType.GetTypeInfo().GetConstructor(new Type[] {typeof(IServiceProvider)});

        private static readonly Func<IServiceProvider, IAspectContext> Factory = CreateFactory();

        private readonly IProxyServiceProvider serviceProvider;

        public AspectContextFactory(IProxyServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IAspectContext Create()
        {
            return Factory(serviceProvider);
        }

        private static Func<IServiceProvider, IAspectContext> CreateFactory()
        {
            var paramter = Expression.Parameter(typeof(IServiceProvider));
            var createInsatnce = Expression.New(constructorInfo, paramter);
            var lambda = Expression.Lambda<Func<IServiceProvider, IAspectContext>>(createInsatnce, paramter);
            return lambda.Compile();
        }
    }
}