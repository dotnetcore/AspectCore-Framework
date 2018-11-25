using System;
using System.Diagnostics;
using System.Threading.Tasks;
using AspectCore.Core.Utils;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    internal sealed class AspectActivator : IAspectActivator
    {
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectBuilderFactory _aspectBuilderFactory;
        private readonly IAspectExceptionWrapper _aspectExceptionWrapper;

        public AspectActivator(IAspectContextFactory aspectContextFactory, IAspectBuilderFactory aspectBuilderFactory, IAspectExceptionWrapper aspectExceptionWrapper)
        {
            _aspectContextFactory = aspectContextFactory;
            _aspectBuilderFactory = aspectBuilderFactory;
            _aspectExceptionWrapper = aspectExceptionWrapper;
        }

        public TResult Invoke<TResult>(AspectActivatorContext activatorContext)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
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

        public async Task<TResult> InvokeTask<TResult>(AspectActivatorContext activatorContext)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                await aspectBuilder.Build()(context);
                var result = context.ReturnValue;
                if (result is Task<TResult> taskWithResult)
                {
                    return await taskWithResult;
                }
                else if (result is Task task)
                {
                    await task;
                    return default(TResult);
                }
                else
                {
                    throw _aspectExceptionWrapper.Wrap(context, new InvalidCastException(
                        $"Unable to cast object of type '{result.GetType()}' to type '{typeof(Task<TResult>)}'."));
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

        public async ValueTask<TResult> InvokeValueTask<TResult>(AspectActivatorContext activatorContext)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                await aspectBuilder.Build()(context);
                return await (ValueTask<TResult>) context.ReturnValue;
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