using System.Reflection;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 拦截条件
    /// </summary>
    /// <param name="method">要拦截的方法</param>
    /// <returns>是否需要拦截以生成代理</returns>
    public delegate bool AspectPredicate(MethodInfo method);
}
