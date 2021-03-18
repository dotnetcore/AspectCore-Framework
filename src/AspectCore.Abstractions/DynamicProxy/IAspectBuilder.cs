using System;
using System.Collections.Generic;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 构建拦截管道
    /// </summary>
    [NonAspect]
    public interface IAspectBuilder
    {
        /// <summary>
        /// 拦截管道中的中间件集合
        /// </summary>
        IEnumerable<Func<AspectDelegate, AspectDelegate>> Delegates { get; }

        /// <summary>
        /// 构建拦截管道
        /// </summary>
        /// <returns>表示拦截处理管道的委托</returns>
        AspectDelegate Build();
    }
}
