using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截器接口
    /// </summary>
    [NonAspect]
    public interface IInterceptor
    {
        /// <summary>
        /// 是否可多使用
        /// </summary>
        bool AllowMultiple { get; }

        /// <summary>
        /// 是否可继承
        /// </summary>
        bool Inherited { get; set; }

        /// <summary>
        /// 排序号,标识拦截顺序
        /// </summary>
        int Order { get; set; }

        /// <summary>
        /// 增强的具体业务逻辑
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <param name="next">后续处理拦截上下文所构建的委托对象</param>
        /// <returns>异步任务</returns>
        Task Invoke(AspectContext context, AspectDelegate next);
    }
}
