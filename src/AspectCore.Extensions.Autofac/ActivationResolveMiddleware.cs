using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Resolving.Pipeline;

namespace AspectCore.Extensions.Autofac
{
    /// <summary>
    /// 
    /// </summary>
    public class ActivationResolveMiddleware : IResolveMiddleware
    {
        private static readonly List<string> excepts = new List<string>
        {
            "Microsoft.Extensions.Logging",
            "Microsoft.Extensions.Options",
            "System",
            "System.*",
            "IHttpContextAccessor",
            "ITelemetryInitializer",
            "IHostingEnvironment",
            "Autofac.*",
            "Autofac"
        };

        private readonly Parameter[] _defaultConstructorParameters = new Parameter[] { new AutowiringParameter(), new DefaultValueParameter() };

        /// <summary>
        /// Gets a singleton instance of the middleware.
        /// </summary>
        public static ActivationResolveMiddleware Instance { get; } = new ActivationResolveMiddleware();

        /// <summary>
        /// Activation
        /// </summary>
        public PipelinePhase Phase => PipelinePhase.Activation;

        public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
        {
            next(context);
            if (context.Instance == null || context.Instance.IsProxy())
            {
                return;
            }
            if (!(context.Registration.Activator is ReflectionActivator
                  || context.Registration.Activator is DelegateActivator
                  || context.Registration.Activator is InstanceActivator))
            {
                return;
            }
            var limitType = context.Instance.GetType();
            if (!limitType.GetTypeInfo().CanInherited())
            {
                return;
            }
            if (excepts.Any(x => limitType.Name.Matches(x)) || excepts.Any(x => limitType.Namespace.Matches(x)))
            {
                return;
            }
            var services = context.Registration.Services.Select(x => ((IServiceWithType)x).ServiceType).ToList();
            if (!services.All(x => x.GetTypeInfo().CanInherited()) || services.All(x => x.GetTypeInfo().IsNonAspect()))
            {
                return;
            }
            var aspectValidator = new AspectValidatorBuilder(context.Resolve<IAspectConfiguration>()).Build();
            if (services.All(x => !aspectValidator.Validate(x, true)) && !aspectValidator.Validate(limitType, false))
            {
                return;
            }
            var proxyTypeGenerator = context.Resolve<IProxyTypeGenerator>();
            Type proxyType; object instance;
            var interfaceType = services.FirstOrDefault(x => x.GetTypeInfo().IsInterface);
            if (interfaceType == null)
            {
                var baseType = services.FirstOrDefault(x => x.GetTypeInfo().IsClass) ?? limitType;
                proxyType = proxyTypeGenerator.CreateClassProxyType(baseType, limitType);

                //Autofac.Core.Activators.Reflection.ReflectionActivator 
                var constructorSelector = new MostParametersConstructorSelector();
                var constructorFinder = new DefaultConstructorFinder(type => type.GetTypeInfo().DeclaredConstructors.ToArray());
                var availableConstructors = constructorFinder.FindConstructors(proxyType);

                if (availableConstructors.Length == 0)
                {
                    throw new NoConstructorsFoundException(proxyType, $"No constructors on type '{proxyType}' can be found with the constructor finder '{constructorFinder}'.");
                }

                var binders = new ConstructorBinder[availableConstructors.Length];
                for (var idx = 0; idx < availableConstructors.Length; idx++)
                {
                    binders[idx] = new ConstructorBinder(availableConstructors[idx]);
                }

                var allBindings = GetAllBindings(binders, context, context.Parameters);
                var selectedBinding = constructorSelector.SelectConstructorBinding(allBindings, context.Parameters);
                instance = selectedBinding.Instantiate();
            }
            else
            {
                proxyType = proxyTypeGenerator.CreateInterfaceProxyType(interfaceType, limitType);
                instance = Activator.CreateInstance(proxyType, new object[] { context.Resolve<IAspectActivatorFactory>(), context.Instance });
            }

            var propertyInjector = context.Resolve<IPropertyInjectorFactory>().Create(instance.GetType());
            propertyInjector.Invoke(instance);
            context.Instance = instance;
        }

        private IEnumerable<Parameter> EnumerateParameters(IEnumerable<Parameter> parameters)
        {
            foreach (var param in parameters)
            {
                yield return param;
            }

            foreach (var defaultParam in _defaultConstructorParameters)
            {
                yield return defaultParam;
            }
        }

        private BoundConstructor[] GetAllBindings(ConstructorBinder[] availableConstructors, IComponentContext context, IEnumerable<Parameter> parameters)
        {
            // Most often, there will be no `parameters` and/or no `_defaultParameters`; in both of those cases we can avoid allocating.
            var prioritisedParameters = parameters.Any() ? EnumerateParameters(parameters) : _defaultConstructorParameters;

            var boundConstructors = new BoundConstructor[availableConstructors.Length];
            var validBindings = availableConstructors.Length;

            for (var idx = 0; idx < availableConstructors.Length; idx++)
            {
                var bound = availableConstructors[idx].Bind(prioritisedParameters, context);

                boundConstructors[idx] = bound;

                if (!bound.CanInstantiate)
                {
                    validBindings--;
                }
            }

            if (validBindings == 0)
            {
                var reasons = new StringBuilder();

                foreach (var invalid in boundConstructors.Where(cb => !cb.CanInstantiate))
                {
                    reasons.AppendLine();
                    reasons.Append(invalid.Description);
                }

                throw new DependencyResolutionException($"No constructors on type '{0}' can be found with the constructor finder '{reasons}'.");
            }

            return boundConstructors;
        }
    }
}