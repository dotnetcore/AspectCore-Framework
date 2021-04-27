using System.Reflection;
using AspectCore.DependencyInjection;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.Configuration;

namespace AspectCore.Extensions.Configuration
{
    /// <summary>
    /// 配置绑定解析
    /// </summary>
    public sealed class ConfigurationBindResolveCallback : IServiceResolveCallback
    {
        private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// 解析配置到对象相关字段
        /// </summary>
        /// <param name="resolver">服务解析 </param>
        /// <param name="instance">实例</param>
        /// <param name="service">服务描述</param>
        /// <returns>解析后的对象</returns>
        public object Invoke(IServiceResolver resolver, object instance, ServiceDefinition service)
        {
            if (instance == null || instance is IConfiguration)
            {
                return instance;
            }

            var instanceType = instance.GetType();
            var configuration = resolver.ResolveRequired<IConfiguration>();
            foreach (var field in instanceType.GetFields(_flags))
            {
                var reflector = field.GetReflector();
                var configurationMetadata = reflector.GetCustomAttribute<ConfigurationMetadataAttribute>();
                if (configurationMetadata == null)
                {
                    continue;
                }

                var section = configurationMetadata.GetSection();
                if (configurationMetadata.Type == ConfigurationBindType.Value)
                {
                    var key = section == null ? configurationMetadata.Key : section + ":" + configurationMetadata.Key;
                    reflector.SetValue(instance, configuration.GetValue(field.FieldType, key));
                }
                else
                {
                    var configurationSection = section == null ? configuration : configuration.GetSection(section);
                    reflector.SetValue(instance, configurationSection.Get(field.FieldType));
                }
            }

            return instance;
        }
    }
}