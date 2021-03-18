using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 处理拦截上下文的拦截委托
    /// </summary>
    /// <param name="context">拦截上下文</param>
    /// <returns>异步任务</returns>
    public delegate Task AspectDelegate(AspectContext context);
}
