using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 构建拦截管道
    /// </summary>
    [NonAspect]
    public sealed class AspectBuilder : IAspectBuilder
    {
        private readonly IList<Func<AspectDelegate, AspectDelegate>> _delegates;
        private readonly AspectDelegate _complete;
        private AspectDelegate _aspectDelegate;

        /// <summary>
        /// 构建拦截管道
        /// </summary>
        /// <param name="complete">末端拦截委托</param>
        /// <param name="delegates">拦截中间件集合</param>
        public AspectBuilder(AspectDelegate complete, IList<Func<AspectDelegate, AspectDelegate>> delegates)
        {
            _complete = complete ?? throw new ArgumentNullException(nameof(complete));
            _delegates = delegates ?? new List<Func<AspectDelegate, AspectDelegate>>();
        }

        /// <summary>
        /// 拦截管道中的拦截中间件集合
        /// </summary>
        public IEnumerable<Func<AspectDelegate, AspectDelegate>> Delegates => _delegates;

        /// <summary>
        /// 添加拦截中间件到管道中
        /// </summary>
        /// <param name="interceptorInvoke">拦截中间件</param>
        public void AddAspectDelegate(Func<AspectContext, AspectDelegate, Task> interceptorInvoke)
        {
            if (interceptorInvoke == null)
            {
                throw new ArgumentNullException(nameof(interceptorInvoke));
            }
            _delegates.Add(next => context => interceptorInvoke(context, next));
        }

        /// <summary>
        /// 构建拦截管道
        /// </summary>
        /// <returns></returns>
        public AspectDelegate Build()
        {
            if (_aspectDelegate != null)
            {
                return _aspectDelegate;
            }
            AspectDelegate invoke = _complete;
            var count = _delegates.Count;
            for (var i = count - 1; i > -1; i--)
            {
                invoke = _delegates[i](invoke);
            }
            return (_aspectDelegate = invoke);
        }
    }
}