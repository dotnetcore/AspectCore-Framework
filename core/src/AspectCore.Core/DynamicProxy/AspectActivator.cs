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

        public AspectActivator(IAspectContextFactory aspectContextFactory, IAspectBuilderFactory aspectBuilderFactory,
            IAspectExceptionWrapper aspectExceptionWrapper)
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