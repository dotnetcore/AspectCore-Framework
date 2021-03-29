using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 基于重写特征的验证处理器
    /// </summary>
    public sealed class OverwriteAspectValidationHandler : IAspectValidationHandler
    {
        /// <summary>
        /// 排序号，表示处理验证的顺序
        /// </summary>
        public int Order { get; } = 1;

        /// <summary>
        /// 检查是否需要被代理
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="next">后续的验证处理委托</param>
        /// <returns>结果</returns>
        public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
        {
            var method = context.Method;
            var declaringType = method.DeclaringType.GetTypeInfo();

            //如果被代理的方法的声明类型不可被继承则返回false
            if (!declaringType.CanInherited())
            {
                return false;
            }
            if (method.IsNonAspect())
            {
                return false;
            }
            if (!method.IsVisibleAndVirtual())
            {
                if (context.StrictValidation)
                {
                    return false;
                }

                //newslot标识脱离了基类虚函数的那一套链，等同C#中的new
                //MethodAttributes.NewSlot指示此方法总是获取 vtable 中的新槽
                if (!method.Attributes.HasFlag(MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final))
                    return false;
            }

            return next(context);
        }
    }
}