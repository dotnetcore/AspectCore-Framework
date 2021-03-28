using System;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    public static class AspectValidatorExtensions
    {
        /// <summary>
        /// 检查类型是否可以被代理
        /// </summary>
        /// <param name="aspectValidator">拦截验证器</param>
        /// <param name="type">类型</param>
        /// <param name="isStrictValidation">模式</param>
        /// <returns>是否需代理</returns>
        public static bool Validate(this IAspectValidator aspectValidator, Type type, bool isStrictValidation)
        {
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            if (type == null)
            {
                return false;
            }
            var typeInfo = type.GetTypeInfo();


            if (typeInfo.IsNonAspect()
                || typeInfo.IsProxyType()
                || typeInfo.IsValueType
                || typeInfo.IsEnum
                || !typeInfo.IsVisible())
            {
                return false;
            }
            //类型不可继承
            if (typeInfo.IsClass && !typeInfo.CanInherited())
            {
                return false;
            }

            //检查类中是否由需要被代理的方法
            foreach (var method in typeInfo.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (aspectValidator.Validate(method, isStrictValidation))
                {
                    return true;
                }
            }

            //检查实现接口中的方法,是否需要被代理
            foreach (var interfaceType in typeInfo.GetInterfaces())
            {
                if (aspectValidator.Validate(interfaceType, isStrictValidation))
                {
                    return true;
                }
            }

            var baseType = typeInfo.BaseType;

            if (baseType == null || baseType == typeof(object))
            {
                return false;
            }

            //检查基类方法，查看是否需要被代理
            return aspectValidator.Validate(baseType, isStrictValidation);
        }
    }
}