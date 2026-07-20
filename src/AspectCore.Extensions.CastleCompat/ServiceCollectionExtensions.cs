using System;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.CastleCompat
{
    /// <summary>
    /// Extension methods for registering Castle interceptors in AspectCore's MSDI pipeline.
    /// These methods enable a gradual migration path from Castle/Windsor to AspectCore.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers a Castle <see cref="Castle.DynamicProxy.IInterceptor"/> to run inside
        /// AspectCore's interception pipeline for services matching the specified predicate.
        /// </summary>
        /// <typeparam name="TCastleInterceptor">
        /// The Castle interceptor type. Must have a parameterless constructor.
        /// </typeparam>
        /// <param name="services">The service collection.</param>
        /// <param name="predicate">
        /// A predicate that determines which services this interceptor applies to.
        /// If null, applies to all services (equivalent to <c>Predicates.ForService("*")</c>).
        /// </param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// // Register a Castle interceptor for all services containing "Service"
        /// services.AddCastleInterceptor&lt;MyCastleLoggingInterceptor&gt;(
        ///     Predicates.ForService("*Service*"));
        /// 
        /// // Register for all services
        /// services.AddCastleInterceptor&lt;MyCastleInterceptor&gt;();
        /// </code>
        /// </example>
        public static IServiceCollection AddCastleInterceptor<TCastleInterceptor>(
            this IServiceCollection services,
            AspectPredicate predicate = null)
            where TCastleInterceptor : Castle.DynamicProxy.IInterceptor, new()
        {
            var castleInterceptor = new TCastleInterceptor();
            return AddCastleInterceptor(services, castleInterceptor, predicate);
        }

        /// <summary>
        /// Registers a Castle <see cref="Castle.DynamicProxy.IInterceptor"/> instance to run
        /// inside AspectCore's interception pipeline for services matching the specified predicate.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="castleInterceptor">The Castle interceptor instance.</param>
        /// <param name="predicate">
        /// A predicate that determines which services this interceptor applies to.
        /// If null, applies to all services.
        /// </param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddCastleInterceptor(
            this IServiceCollection services,
            Castle.DynamicProxy.IInterceptor castleInterceptor,
            AspectPredicate predicate = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (castleInterceptor == null) throw new ArgumentNullException(nameof(castleInterceptor));

            services.ConfigureDynamicProxy(config =>
            {
                if (predicate != null)
                {
                    config.Interceptors.AddTyped(
                        typeof(CastleInterceptorAdapter),
                        new object[] { castleInterceptor },
                        predicate);
                }
                else
                {
                    config.Interceptors.AddTyped(
                        typeof(CastleInterceptorAdapter),
                        new object[] { castleInterceptor });
                }
            });

            return services;
        }

        /// <summary>
        /// Registers multiple Castle interceptors for services matching the specified predicate.
        /// Interceptors are invoked in the order they are provided.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="predicate">
        /// A predicate that determines which services these interceptors apply to.
        /// </param>
        /// <param name="castleInterceptors">The Castle interceptor instances.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddCastleInterceptors(
            this IServiceCollection services,
            AspectPredicate predicate,
            params Castle.DynamicProxy.IInterceptor[] castleInterceptors)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (castleInterceptors == null) throw new ArgumentNullException(nameof(castleInterceptors));

            foreach (var interceptor in castleInterceptors)
            {
                services.AddCastleInterceptor(interceptor, predicate);
            }

            return services;
        }
    }
}
