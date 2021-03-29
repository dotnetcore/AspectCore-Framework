using System;
using System.Reflection;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 服务验证
    /// </summary>
    internal class ServiceValidator
    {
        private readonly IAspectValidator _aspectValidator;

        /// <summary>
        /// 服务验证
        /// </summary>
        /// <param name="aspectValidatorBuilder">验证构建器</param>
        internal ServiceValidator(IAspectValidatorBuilder aspectValidatorBuilder)
        {
            _aspectValidator = aspectValidatorBuilder.Build();
        }

        /// <summary>
        /// 检查是否需要代理
        /// </summary>
        /// <param name="definition">服务描述</param>
        /// <param name="implementationType">实现类型</param>
        /// <returns>验证是否通过</returns>
        internal bool TryValidate(ServiceDefinition definition, out Type implementationType)
        {
            implementationType = null;

            if (definition.ServiceType.GetTypeInfo().IsNonAspect())
            {
                return false;
            }

            implementationType = definition.GetImplementationType();

            if (implementationType == null || implementationType == typeof(object))
            {
                return false;
            }
            //类型不是类或委托
            if (!implementationType.GetTypeInfo().IsClass)
            {
                return false;
            }

            if (definition.ServiceType.GetTypeInfo().IsClass)
            {
                if (!(definition is TypeServiceDefinition))
                {
                    return false;
                }
                //类型不可被继承
                if (!implementationType.GetTypeInfo().CanInherited())
                {
                    return false;
                }
            }

            return _aspectValidator.Validate(definition.ServiceType, true) || _aspectValidator.Validate(implementationType, false);
        }
    }
}