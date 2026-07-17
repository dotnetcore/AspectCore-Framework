using AspectCore.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.Utils;

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
                    ExceptionDispatchInfo.Capture(task.Exception.InnerException!).Throw();
                }
                if (!task.IsCompleted)
                {
                    // try to avoid potential deadlocks.
                    NoSyncContextScope.Run(task);
                    // task.GetAwaiter().GetResult();
                }

                return (TResult)context.ReturnValue;
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
                        return await taskWithResult;
                    case Task task:
                        await task;
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
                        return await taskWithResult;
                    case ValueTask task:
                        await task;
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

        public IAsyncEnumerable<TResult> InvokeAsyncEnumerable<TResult>(AspectActivatorContext activatorContext)
        {
            return InvokeAsyncEnumerableCore<TResult>(activatorContext);
        }

        private async IAsyncEnumerable<TResult> InvokeAsyncEnumerableCore<TResult>(
            AspectActivatorContext activatorContext,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var context = _aspectContextFactory.CreateContext(activatorContext);
            try
            {
                var asyncEnumerable = await CreateAsyncEnumerable<TResult>(context);
                if (asyncEnumerable == null)
                {
                    yield break;
                }

                var enumerator = asyncEnumerable.WithCancellation(cancellationToken).GetAsyncEnumerator();
                try
                {
                    while (true)
                    {
                        TResult item;
                        try
                        {
                            if (!await enumerator.MoveNextAsync())
                            {
                                break;
                            }

                            item = enumerator.Current;
                        }
                        catch (Exception ex)
                        {
                            if (!_aspectConfiguration.ThrowAspectException || ex is AspectInvocationException _)
                                throw;

                            throw new AspectInvocationException(context, ex);
                        }

                        yield return item;
                    }
                }
                finally
                {
                    try
                    {
                        await enumerator.DisposeAsync();
                    }
                    catch (Exception ex)
                    {
                        if (!_aspectConfiguration.ThrowAspectException || ex is AspectInvocationException _)
                            throw;

                        throw new AspectInvocationException(context, ex);
                    }
                }
            }
            finally
            {
                _aspectContextFactory.ReleaseContext(context);
            }
        }

        private async Task<IAsyncEnumerable<TResult>> CreateAsyncEnumerable<TResult>(AspectContext context)
        {
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
                        return null;
                    case IAsyncEnumerable<TResult> asyncEnumerable:
                        return asyncEnumerable;
                    default:
                        throw new AspectInvalidCastException(context, $"Unable to cast object of type '{context.ReturnValue.GetType()}' to type '{typeof(IAsyncEnumerable<TResult>)}'.");
                }
            }
            catch (Exception ex)
            {
                if (!_aspectConfiguration.ThrowAspectException || ex is AspectInvocationException _)
                    throw;

                throw new AspectInvocationException(context, ex);
            }
        }
    }
}
