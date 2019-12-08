namespace AspectCore.DependencyInjection
{
    public class PropertyInjectorCallback : IServiceResolveCallback
    {
        public object Invoke(IServiceResolver resolver, object instance, ServiceDefinition service)
        {
            if (instance == null || !service.RequiredPropertyInjection())
            {
                return instance;
            }
            var injectorFactory = resolver.Resolve<IPropertyInjectorFactory>();
            var injector = injectorFactory.Create(instance.GetType());
            injector.Invoke(instance);
            return instance;
        }
    }
}