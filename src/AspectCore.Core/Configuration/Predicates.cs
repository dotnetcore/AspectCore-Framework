using System;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 构造条件的工具类
    /// </summary>
    public static class Predicates
    {
        /// <summary>
        /// 构造一个条件,此条件判断声明此方法的类型的名称空间是否匹配nameSpace参数指定的模式
        /// </summary>
        /// <param name="nameSpace">名称空间模式字符串</param>
        /// <returns>条件</returns>
        public static AspectPredicate ForNameSpace(string nameSpace)
        {
            if (nameSpace == null)
            {
                throw new ArgumentNullException(nameof(nameSpace));
            }

            return method => method.DeclaringType.Namespace.Matches(nameSpace);
        }

        /// <summary>
        /// 构造一个条件,此条件判断声明此方法的类型的名称是否匹配service参数指定的模式
        /// </summary>
        /// <param name="service">服务名称模式字符串</param>
        /// <returns>条件</returns>
        public static AspectPredicate ForService(string service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return method =>
            {
                if (method.DeclaringType.Name.Matches(service))
                {
                    return true;
                }

                var declaringType = method.DeclaringType;
                var fullName = declaringType.FullName ?? $"{declaringType.Name}.{declaringType.Name}";
                return fullName.Matches(service);
            };
        }

        /// <summary>
        /// 构造一个条件,此条件判断方法名称是否匹配method参数指定的模式
        /// </summary>
        /// <param name="method">方法名称模式字符串</param>
        /// <returns>条件</returns>
        public static AspectPredicate ForMethod(string method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return methodInfo => methodInfo.Name.Matches(method);
        }

        /// <summary>
        /// 构造一个条件,此条件判断指定服务下的方法名称是否匹配method参数指定的模式
        /// </summary>
        /// <param name="service">服务名称模式字符串</param>
        /// <param name="method">方法名称模式字符串</param>
        /// <returns>条件</returns>
        public static AspectPredicate ForMethod(string service, string method)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return methodInfo => ForService(service)(methodInfo) && methodInfo.Name.Matches(method);
        }

        /// <summary>
        /// 构造一个条件,判断方法的声明类型是否由baseOrInterfaceType类型派生
        /// </summary>
        /// <param name="baseOrInterfaceType">基类型或接口</param>
        /// <returns>条件</returns>
        public static AspectPredicate Implement(Type baseOrInterfaceType)
        {
            if (baseOrInterfaceType == null)
            {
                throw new ArgumentNullException(nameof(baseOrInterfaceType));
            }

            if (!(baseOrInterfaceType.IsClass || baseOrInterfaceType.IsInterface))
            {
                throw new ArgumentException("The base type must be class or interface.");
            }

            if (baseOrInterfaceType.IsSealed)
            {
                throw new ArgumentException("The base type is not allowed to be Sealed.");
            }
            
            return methodInfo => baseOrInterfaceType.IsAssignableFrom(methodInfo.DeclaringType);
        }
    }
}