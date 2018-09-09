using System;
using System.Reflection;
using AspectCore.Injector;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.Configuration;

namespace AspectCore.Extensions.Configuration
{
    public sealed class ConfigurationBindResolveCallback : IServiceResolveCallback
    {
        private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public object Invoke(IServiceResolver resolver, object instance, ServiceDefinition service)
        {
            if (instance == null)
            {
                return instance;
            }

            var instanceType = instance.GetType();
            var configuration = resolver.ResolveRequired<IConfiguration>();
            foreach (var field in instanceType.GetFields(_flags))
            {
                var reflector = field.GetReflector();
                var configurationBinding = reflector.GetCustomAttribute<ConfigurationBinding>();
                if (configurationBinding != null)
                {
                    var fieldValue = Activator.CreateInstance(field.FieldType);
                    configuration.Bind(configurationBinding.Section, fieldValue);
                    reflector.SetValue(instance, fieldValue);
                    continue;
                }
                var configurationValue = reflector.GetCustomAttribute<ConfigurationValue>();
                if (configurationValue != null)
                {
                    reflector.SetValue(instance, configuration.GetValue(field.FieldType, configurationValue.Key));
                }
            }

            return instance;
        }
    }
}