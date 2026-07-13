using System.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectBuilderFactory
    {
        IAspectBuilder Create(AspectContext context);

        /// <summary>
        /// Gets the aspect builder for the given method pair, using the method pair directly as the cache key.
        /// This avoids the need to construct an <see cref="AspectContext"/> for the lookup.
        /// </summary>
        IAspectBuilder GetBuilder(MethodInfo serviceMethod, MethodInfo implementationMethod);

        /// <summary>
        /// Gets the aspect builder for the given method triple, using the predicate method
        /// for interceptor selection. This avoids the need to construct an <see cref="AspectContext"/>
        /// for the lookup. Used by source-generated proxies for inline activation.
        /// </summary>
        IAspectBuilder GetBuilder(MethodInfo serviceMethod, MethodInfo implementationMethod, MethodInfo predicateMethod);
    }
}
