using AspectCore.Configuration;
using AspectCore.Core.Utils;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    internal sealed class AspectActivator : IAspectActivator
    {
        private readonly IAspectContextFactory _aspectContextFactory;
        private readonly IAspectBuilderFactory _aspectBuilderFactory;
        private readonly IAspectConfiguration _aspectConfiguration;

        public AspectActivator(IAspectContextFactory aspectContextFactory, IAspectBuilderFactory aspectBuilderFactory, IAspectConfiguration aspectConfiguration)
        {
            _aspectContextFactory = aspectContextFactory;
            _aspectBuilderFactory = aspectBuilderFactory;
            _aspectConfiguration = aspectConfiguration;
        }

        public TResult Invoke<TResult>(AspectActivatorContext activatorContext)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var aspectBuilder = _aspectBuilderFactory.Create(context);
                var task = aspectBuilder.Build()(context);
                if (task.IsFaulted)
                {
                    ExceptionDispatchInfo.Capture(task.Exception.InnerException).Throw();
                }
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
                if (!_aspectConfiguration.ThrowAspectException || ex is AspectInvocationException _)
                    throw;

                throw new AspectInvocationException(context, ex);
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
                    ExceptionDispatchInfo.Capture(invoke.Exception.InnerException).Throw();
                }

                if (!invoke.IsCompleted)
                {
                    await invoke;
                }

                switch (context.ReturnValue)
                {
                    case null:
                        return default;
                    case Task<TResult> taskWithResult:
                        return taskWithResult.Result;
                    case Task _:
                        return default;
                    default:
                        throw new AspectInvalidCastException(context, $"Unable to cast object of type '{context.ReturnValue.GetType()}' to type '{typeof(Task<TResult>)}'.");
                }
            }
            catch (Exception ex)
            {
                if (!_aspectConfiguration.ThrowAspectException || ex is AspectInvocationException _) 
                    throw;

                throw new AspectInvocationException(context, ex);
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
                    ExceptionDispatchInfo.Capture(invoke.Exception.InnerException).Throw();
                }
                
                if (!invoke.IsCompleted)
                {
                    await invoke;
                }

                switch (context.ReturnValue)
                {
                    case null:
                        return default;
                    case ValueTask<TResult> taskWithResult:
                        return taskWithResult.Result;
                    case ValueTask task:
                        return default;
                    default:
                        throw new AspectInvalidCastException(context, $"Unable to cast object of type '{context.ReturnValue.GetType()}' to type '{typeof(ValueTask<TResult>)}'.");
                }
            }
            catch (Exception ex)
            {
                if (!_aspectConfiguration.ThrowAspectException || ex is AspectInvocationException _)
                    throw;

                throw new AspectInvocationException(context, ex);
            }
            finally
            {
                _aspectContextFactory.ReleaseContext(context);
            }
        }
    }
}