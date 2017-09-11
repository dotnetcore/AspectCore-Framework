namespace AspectCore.Extensions.Autofac
{
    //public static class RegistrationBuilderExtensions
    //{
    //    public static void AsInterfacesProxy<TLimit, TRegistrationStyle>(this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration)
    //    {
    //        if (registration == null)
    //        {
    //            throw new ArgumentNullException(nameof(registration));
    //        }

    //        registration.ActivatorData.ConfigurationActions.Add((t, rb) => rb.AsInterfacesProxy());
    //    }

    //    public static void AsClassProxy<TLimit, TRegistrationStyle>(this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> registration)
    //    {
    //        if (registration == null)
    //        {
    //            throw new ArgumentNullException(nameof(registration));
    //        }

    //        registration.ActivatorData.ConfigurationActions.Add((t, rb) => rb.AsClassProxy());
    //    }

    //    public static void AsInterfacesProxy<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration)
    //        where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
    //    {
    //        if (registration == null)
    //        {
    //            throw new ArgumentNullException(nameof(registration));
    //        }

    //        var activatorData = registration.ActivatorData;

    //        var interfaceTypes = registration.RegistrationData.Services.Select(s => s.GetServiceType()).Where(type => type.GetTypeInfo().IsInterface).ToArray();

    //        if (interfaceTypes.Length == 0)
    //        {
    //            return;
    //        }

    //        foreach(var interfaceType in interfaceTypes)
    //        {
    //            AutofacRealServiceProvider.MapActivatorData(interfaceType, activatorData);
    //        }

    //        registration.OnActivating(args =>
    //        {
    //            var parameters = args.Parameters.ToList();

    //            parameters.Add(new ResolvedParameter((pi, ctx) => pi.ParameterType == typeof(IServiceProvider), (pi, ctx) => ctx.Resolve<IServiceProvider>()));

    //            var proxyGenerator = args.Context.Resolve<IProxyGenerator>();

    //            var proxyType = proxyGenerator.CreateClassProxyType(activatorData.ImplementationType, activatorData.ImplementationType);

    //            var proxyActivator = new ReflectionActivator(proxyType, activatorData.ConstructorFinder,
    //                activatorData.ConstructorSelector, EmptyArray<Parameter>.Value, activatorData.ConfiguredProperties);

    //            var proxyValue = proxyActivator.ActivateInstance(args.Context, parameters);

    //            args.ReplaceInstance(proxyValue);
    //        });
    //    }

    //    public static void AsClassProxy<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registration)
    //        where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
    //    {
    //        if (registration == null)
    //        {
    //            throw new ArgumentNullException(nameof(registration));
    //        }

    //        var activatorData = registration.ActivatorData;

    //        var serviceType = registration.RegistrationData.Services.Select(s => s.GetServiceType()).Where(type => type.GetTypeInfo().IsClass).First();

    //        if (serviceType == null)
    //        {
    //            return;
    //        }

    //        AutofacRealServiceProvider.MapActivatorData(serviceType, activatorData);

    //        registration.OnActivating(args =>
    //        {
    //            var parameters = args.Parameters.ToList();

    //            parameters.Add(new ResolvedParameter((pi, ctx) => pi.ParameterType == typeof(IServiceProvider), (pi, ctx) => ctx.Resolve<IServiceProvider>()));

    //            var proxyGenerator = args.Context.Resolve<IProxyGenerator>();

    //            var proxyType = proxyGenerator.CreateClassProxyType(serviceType, activatorData.ImplementationType);

    //            var proxyActivator = new ReflectionActivator(proxyType, activatorData.ConstructorFinder,
    //                activatorData.ConstructorSelector, EmptyArray<Parameter>.Value, activatorData.ConfiguredProperties);

    //            var proxyValue = proxyActivator.ActivateInstance(args.Context, parameters);

    //            args.ReplaceInstance(proxyValue);
    //        });
    //    }
    //}
}
