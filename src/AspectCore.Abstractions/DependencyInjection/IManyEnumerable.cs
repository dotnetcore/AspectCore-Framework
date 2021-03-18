using System.Collections;
using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 定义一个实现IEnumerable<T>, IEnumerable的接口，此接口标注了NonAspect, NonCallback特性
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [NonAspect, NonCallback]
    public interface IManyEnumerable<out T> : IEnumerable<T>, IEnumerable
    {
    }
}