using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截验证
    /// </summary>
    [NonAspect]
    public sealed class AspectValidator : IAspectValidator
    {
        private readonly AspectValidationDelegate _aspectValidationDelegate;

        /// <summary>
        /// 拦截验证
        /// </summary>
        /// <param name="aspectValidationDelegate">处理AspectValidationContext上下文的委托</param>
        public AspectValidator(AspectValidationDelegate aspectValidationDelegate)
        {
            _aspectValidationDelegate = aspectValidationDelegate;
        }

        /// <summary>
        /// 确定方法是否需要被代理
        /// </summary>
        /// <param name="method">待检查的方法</param>
        /// <param name="isStrictValidation">检查模式</param>
        /// <returns>true 需要代理 false 不需要</returns>
        public bool Validate(MethodInfo method, bool isStrictValidation)
        {
            if (method == null)
            {
                return false;
            }

            var context = new AspectValidationContext { Method = method, StrictValidation = isStrictValidation };
            if (_aspectValidationDelegate(context))
            {
                return true;
            }

            var declaringTypeInfo = method.DeclaringType.GetTypeInfo();
            if (!declaringTypeInfo.IsClass)
            {
                return false;
            }

            foreach (var interfaceTypeInfo in declaringTypeInfo.GetInterfaces().Select(x => x.GetTypeInfo()))
            {
                var interfaceMethod = interfaceTypeInfo.GetMethodBySignature(new MethodSignature(method));
                if (interfaceMethod != null)
                {
                    if (Validate(interfaceMethod, isStrictValidation))
                    {
                        return true;
                    }
                }
            }

            var baseType = declaringTypeInfo.BaseType;
            if (baseType == typeof(object) || baseType == null)
            {
                return false;
            }

            var baseMethod = baseType.GetTypeInfo().GetMethodBySignature(new MethodSignature(method));
            return baseMethod != null && Validate(baseMethod, isStrictValidation);
        }
    }
}