using System.Linq;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 针对拦截特性进行判断以确定是否需要代理的处理器
    /// </summary>
    [NonAspect]
    public sealed class AttributeAspectValidationHandler : IAspectValidationHandler
    {
        /// <summary>
        /// 排序号，表示处理验证的顺序
        /// </summary>
        public int Order { get; } = 13;

        /// <summary>
        /// 检查是否需要被代理
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="next">后续的验证处理委托</param>
        /// <returns>结果</returns>
        public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
        {
            var declaringType = context.Method.DeclaringType.GetTypeInfo();

            if (IsAttributeAspect(declaringType) || IsAttributeAspect(context.Method))
            {
                return true;
            }

            return next(context);
        }

        /// <summary>
        /// 判断成员的特性上是否有拦截器特性
        /// </summary>
        /// <param name="member">MemberInfo</param>
        /// <returns>true 有拦截器特性, false无拦截器特性</returns>
        private bool IsAttributeAspect(MemberInfo member)
        {
            return member.CustomAttributes.Any(data => typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(data.AttributeType));
        }
    }
}