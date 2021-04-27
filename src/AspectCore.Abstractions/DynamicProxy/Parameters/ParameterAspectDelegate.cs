using System.Threading.Tasks;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 参数拦截委托。传递一个参数拦截上下文，并对其进行拦截处理
    /// </summary>
    /// <param name="context">参数拦截上下文</param>
    /// <returns>异步任务</returns>
    public delegate Task ParameterAspectDelegate(ParameterAspectContext context);
}
