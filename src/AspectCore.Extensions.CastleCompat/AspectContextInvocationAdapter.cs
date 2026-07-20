using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Castle.DynamicProxy;

namespace AspectCore.Extensions.CastleCompat
{
    /// <summary>
    /// Adapts an AspectCore <see cref="AspectContext"/> + <see cref="AspectDelegate"/>
    /// into a Castle <see cref="IInvocation"/> interface.
    /// 
    /// <para>
    /// This allows Castle-style interceptors to work with AspectCore's pipeline
    /// by presenting the familiar IInvocation API over AspectCore internals.
    /// </para>
    /// </summary>
    internal sealed class AspectContextInvocationAdapter : IInvocation
    {
        private readonly AspectContext _context;
        private readonly AspectDelegate _next;
        private bool _proceeded;

        /// <summary>
        /// If the interceptor calls Proceed() on an async method, this holds the resulting Task.
        /// </summary>
        internal Task AsyncResult { get; private set; }

        public AspectContextInvocationAdapter(AspectContext context, AspectDelegate next)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        /// <inheritdoc />
        public object[] Arguments => _context.Parameters;

        /// <inheritdoc />
        public Type[] GenericArguments
        {
            get
            {
                var method = _context.ServiceMethod;
                return method.IsGenericMethod ? method.GetGenericArguments() : Type.EmptyTypes;
            }
        }

        /// <inheritdoc />
        public object InvocationTarget => _context.Implementation;

        /// <inheritdoc />
        public MethodInfo Method => _context.ServiceMethod;

        /// <inheritdoc />
        public MethodInfo MethodInvocationTarget => _context.ImplementationMethod;

        /// <inheritdoc />
        public object Proxy => _context.Proxy;

        /// <inheritdoc />
        public object ReturnValue
        {
            get => _context.ReturnValue;
            set => _context.ReturnValue = value;
        }

        /// <inheritdoc />
        public Type TargetType => _context.Implementation?.GetType() ?? _context.ServiceMethod.DeclaringType!;

        /// <inheritdoc />
        public object GetArgumentValue(int index) => _context.Parameters[index];

        /// <inheritdoc />
        public MethodInfo GetConcreteMethod() => _context.ImplementationMethod;

        /// <inheritdoc />
        public MethodInfo GetConcreteMethodInvocationTarget() => _context.ImplementationMethod;

        /// <inheritdoc />
        public void Proceed()
        {
            if (_proceeded)
            {
                throw new InvalidOperationException(
                    "Proceed() can only be called once per invocation in the AspectCore adapter.");
            }

            _proceeded = true;
            var task = _next(_context);

            if (task.IsCompleted)
            {
                // Synchronous completion - propagate any exception
                task.GetAwaiter().GetResult();
            }
            else
            {
                // Async completion - store for the adapter to await
                AsyncResult = task;
            }
        }

        /// <inheritdoc />
        public void SetArgumentValue(int index, object value)
        {
            _context.Parameters[index] = value;
        }

        /// <inheritdoc />
        public IInvocationProceedInfo CaptureProceedInfo()
        {
            return new ProceedInfo(this);
        }

        private sealed class ProceedInfo : IInvocationProceedInfo
        {
            private readonly AspectContextInvocationAdapter _adapter;

            public ProceedInfo(AspectContextInvocationAdapter adapter)
            {
                _adapter = adapter;
            }

            public void Invoke()
            {
                _adapter.Proceed();
            }
        }
    }
}
