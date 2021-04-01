using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AspectCore.Core.Utils;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 触发执行拦截管道
    /// </summary>
    [NonAspect]
    internal sealed class AspectActivator : IAspectActivator
    {
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectBuilderFactory _aspectBuilderFactory;
        private readonly IAspectExceptionWrapper _aspectExceptionWrapper;

        /// <summary>
        /// 触发执行拦截管道
        /// </summary>
        /// <param name="aspectContextFactory">拦截上下文工厂</param>
        /// <param name="aspectBuilderFactory">拦截管道构建器工厂</param>
        /// <param name="aspectExceptionWrapper">拦截异常包装类</param>
        public AspectActivator(IAspectContextFactory aspectContextFactory, IAspectBuilderFactory aspectBuilderFactory,
            IAspectExceptionWrapper aspectExceptionWrapper)
        {
            _aspectContextFactory = aspectContextFactory;
            _aspectBuilderFactory = aspectBuilderFactory;
            _aspectExceptionWrapper = aspectExceptionWrapper;
        }

        /// <summary>
        /// 同步执行拦截管道
        /// </summary>
        /// <typeparam name="TResult">返回值的类型</typeparam>
        /// <param name="activatorContext">切面上下文</param>
        /// <returns>返回的值</returns>
        public TResult Invoke<TResult>(AspectActivatorContext activatorContext)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                //构建并执行拦截管道
                var task = aspectBuilder.Build()(context);
                if (task.IsFaulted)
                    throw _aspectExceptionWrapper.Wrap(context, task.Exception.InnerException);
                if (!task.IsCompleted)
                {
                    // try to avoid potential deadlocks.
                    NoSyncContextScope.Run(task);
                    // task.GetAwaiter().GetResult();
                }

                return (TResult) context.ReturnValue;
            }
            catch (Exception ex)
            {
                throw _aspectExceptionWrapper.Wrap(context, ex);
            }
            finally
            {
                _aspectContextFactory.ReleaseContext(context);
            }
        }

        /// <summary>
        /// 异步执行拦截管道
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="activatorContext">切面上下文</param>
        /// <returns>异步结果</returns>
        public async Task<TResult> InvokeTask<TResult>(AspectActivatorContext activatorContext)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                var invoke = aspectBuilder.Build()(context);

                if (invoke.IsFaulted)
                {
                    throw _aspectExceptionWrapper.Wrap(context, invoke.Exception?.InnerException);
                }

                if (!invoke.IsCompleted)
                {
                    await invoke;
                }

                switch (context.ReturnValue)
                {
                    case null:
                        return default(TResult);
                    case Task<TResult> taskWithResult:
                        return taskWithResult.Result;
                    case Task _:
                        return default(TResult);
                    default:
                        throw _aspectExceptionWrapper.Wrap(context, new InvalidCastException(
                            $"Unable to cast object of type '{context.ReturnValue.GetType()}' to type '{typeof(Task<TResult>)}'."));
                }
            }
            catch (Exception ex)
            {
                throw _aspectExceptionWrapper.Wrap(context, ex);
            }
            finally
            {
                _aspectContextFactory.ReleaseContext(context);
            }
        }

        /// <summary>
        /// 异步执行拦截管道
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="activatorContext">切面上下文</param>
        /// <returns>异步结果</returns>
        public async ValueTask<TResult> InvokeValueTask<TResult>(AspectActivatorContext activatorContext)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                var invoke = aspectBuilder.Build()(context);
                
                if (invoke.IsFaulted)
                {
                    throw _aspectExceptionWrapper.Wrap(context, invoke.Exception?.InnerException);
                }
                
                if (!invoke.IsCompleted)
                {
                    await invoke;
                }

                switch (context.ReturnValue)
                {
                    case null:
                        return default(TResult);
                    case ValueTask<TResult> taskWithResult:
                        return taskWithResult.Result;
                    case ValueTask task:
                        return default(TResult);
                    default:
                        throw _aspectExceptionWrapper.Wrap(context, new InvalidCastException(
                            $"Unable to cast object of type '{context.ReturnValue.GetType()}' to type '{typeof(ValueTask<TResult>)}'."));
                }
            }
            catch (Exception ex)
            {
                throw _aspectExceptionWrapper.Wrap(context, ex);
            }
            finally
            {
                _aspectContextFactory.ReleaseContext(context);
            }
        }
    }
}