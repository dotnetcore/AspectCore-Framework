using System.Reflection;

namespace AspectCore.Configuration
{
    /// <summary>
    /// 代表拦截条件的委托
    /// </summary>
    /// <param name="method">要拦截的方法</param>
    /// <returns>是否可以对其拦截</returns>
    public delegate bool AspectPredicate(MethodInfo method);
}
