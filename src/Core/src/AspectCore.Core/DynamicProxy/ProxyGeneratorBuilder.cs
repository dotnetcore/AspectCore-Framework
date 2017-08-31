using System;
using System.Collections.Generic;
using AspectCore.Abstractions;
using AspectCore.Core.Configuration;
using AspectCore.Core.Injector;

namespace AspectCore.Core.DynamicProxy
{
    public sealed class ProxyGeneratorBuilder
    {
        private readonly AspectCoreOptions _aspectCoreOptions;
        private readonly Dictionary<Type, IInterceptorSelector> _selectors;

        public ProxyGeneratorBuilder()
        {
            _aspectCoreOptions = new AspectCoreOptions(null);
            _selectors = new Dictionary<Type, IInterceptorSelector>();
        }

        public ProxyGeneratorBuilder Configure(Action<AspectCoreOptions> options)
        {
            options?.Invoke(_aspectCoreOptions);
            return this;
        }

        public ProxyGeneratorBuilder UseSelector(IInterceptorSelector interceptorSelector)
        {
            if (interceptorSelector == null)
            {
                throw new ArgumentNullException(nameof(interceptorSelector));
            }
            if (!_selectors.ContainsKey(interceptorSelector.GetType()))
            {
                _selectors[interceptorSelector.GetType()] = interceptorSelector;
            }
            return this;
        }

        public IProxyGenerator Build()
        {
            AspectConfigureProvider.AddInterceptorFactories(_aspectCoreOptions.InterceptorFactories);
            AspectConfigureProvider.AddNonAspectPredicates(_aspectCoreOptions.NonAspectPredicates);
            AspectConfigureProvider.AddValidationHandlers(_aspectCoreOptions.AspectValidationHandlers);
            var resolver = _aspectCoreOptions.Services.Build();
            UseSelector(new MethodInterceptorSelector());
            UseSelector(new TypeInterceptorSelector());
            UseSelector(new ConfigureInterceptorSelector(AspectConfigureProvider.Instance, resolver));
            return new ProxyGenerator(
                new ProxyTypeGenerator(
                    new AspectValidatorBuilder(AspectConfigureProvider.Instance)),
                new AspectActivatorFactory(
                    new AspectContextFactory(resolver),
                    new AspectBuilderFactory(
                        new InterceptorCollector(_selectors.Values,
                        new PropertyInjectorFactory(resolver)))));
        }
    }
}