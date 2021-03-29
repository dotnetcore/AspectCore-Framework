using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 参数拦截执行器
    /// </summary>
    internal class ParameterAspectInvoker
    {
        private readonly IList<Func<ParameterAspectDelegate, ParameterAspectDelegate>> delegates = new List<Func<ParameterAspectDelegate, ParameterAspectDelegate>>();

        /// <summary>
        /// 添加委托到参数拦截管道
        /// </summary>
        /// <param name="parameterAspectDelegate">添加的委托</param>
        public void AddDelegate(Func<ParameterAspectContext, ParameterAspectDelegate, Task> parameterAspectDelegate)
        {
            delegates.Add(next => ctx => parameterAspectDelegate(ctx, next));
        }

        /// <summary>
        /// 构建参数拦截管道
        /// </summary>
        /// <returns>参数拦截管道</returns>
        private ParameterAspectDelegate Build()
        {
            ParameterAspectDelegate invoke = ctx => TaskUtils.CompletedTask;

            foreach (var next in delegates.Reverse())
            {
                invoke = next(invoke);
            }

            return invoke;
        }

        /// <summary>
        /// 执行参数拦截管道
        /// </summary>
        /// <param name="context">参数拦截上下文</param>
        /// <returns>异步任务</returns>
        public Task Invoke(ParameterAspectContext context)
        {
            return Build()(context);
        }

        /// <summary>
        /// 清空要执行的拦截委托
        /// </summary>
        public void Reset()
        {
            delegates.Clear();
        }
    }
}