using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Utils;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 代理类型生成器
    /// </summary>
    [NonAspect]
    public sealed class ProxyTypeGenerator : IProxyTypeGenerator
    {
        private readonly IAspectValidator _aspectValidator;
        private readonly ProxyGeneratorUtils _proxyGeneratorUtils;

        /// <summary>
        /// 代理类型生成器
        /// </summary>
        /// <param name="aspectValidatorBuilder">验证器构建者</param>
        public ProxyTypeGenerator(IAspectValidatorBuilder aspectValidatorBuilder)
        {
            if (aspectValidatorBuilder == null)
            {
                throw new ArgumentNullException(nameof(aspectValidatorBuilder));
            }
            _aspectValidator = aspectValidatorBuilder.Build();
            _proxyGeneratorUtils = new ProxyGeneratorUtils();
        }

        /// <summary>
        /// 通过子类代理方式创建代理类型
        /// </summary>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <param name="implementationType">实现类型</param>
        /// <returns>代理类的类型</returns>
        public Type CreateClassProxyType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (!serviceType.GetTypeInfo().IsClass)
            {
                throw new ArgumentException($"Type '{serviceType}' should be class.", nameof(serviceType));
            }
            return _proxyGeneratorUtils.CreateClassProxy(serviceType, implementationType, GetInterfaces(implementationType).ToArray(), _aspectValidator);
        }

        /// <summary>
        /// 创建接口代理类型
        /// </summary>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <returns>由接口代理方式实现的代理类的类型</returns>
        public Type CreateInterfaceProxyType(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (!serviceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException($"Type '{serviceType}' should be interface.", nameof(serviceType));
            }
            return _proxyGeneratorUtils.CreateInterfaceProxy(serviceType, GetInterfaces(serviceType, serviceType).ToArray(), _aspectValidator);
        }

        /// <summary>
        /// 通过接口代理方式创建代理类型
        /// </summary>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <param name="implementationType">实现类型</param>
        /// <returns>由接口代理方式实现的代理类的类型</returns>
        public Type CreateInterfaceProxyType(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (!serviceType.GetTypeInfo().IsInterface)
            {
                throw new ArgumentException($"Type '{serviceType}' should be interface.", nameof(serviceType));
            }
            return _proxyGeneratorUtils.CreateInterfaceProxy(serviceType, implementationType, GetInterfaces(implementationType, serviceType).ToArray(), _aspectValidator);
        }

        private IEnumerable<Type> GetInterfaces(Type type, params Type[] exceptInterfaces)
        {
            var hashSet = new HashSet<Type>(exceptInterfaces);
            foreach (var interfaceType in type.GetTypeInfo().GetInterfaces().Distinct())
            {
                if (!interfaceType.GetTypeInfo().IsVisible())
                {
                    continue;
                }
                //不在要排除的接口集合才返回
                if (!hashSet.Contains(interfaceType))
                {
                    //interfaceType和type包含泛型参数
                    if (interfaceType.GetTypeInfo().ContainsGenericParameters && type.GetTypeInfo().ContainsGenericParameters)
                    {
                        //接口的泛型类型不在要排除的接口集合才返回
                        if (!hashSet.Contains(interfaceType.GetGenericTypeDefinition()))
                            yield return interfaceType;
                    }
                    else
                    {
                        yield return interfaceType;
                    }
                }
            }
        }

    }
}