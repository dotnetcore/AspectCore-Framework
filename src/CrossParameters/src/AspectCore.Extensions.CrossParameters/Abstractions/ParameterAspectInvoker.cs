using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.CrossParameters
{
    internal class ParameterAspectInvoker
    {
        private readonly static Task Completed = Task.FromResult(false);

        private readonly IList<Func<ParameterAspectDelegate, ParameterAspectDelegate>> delegates = new List<Func<ParameterAspectDelegate, ParameterAspectDelegate>>();

        public void AddDelegate(Func<IParameterDescriptor, ParameterAspectContext, ParameterAspectDelegate, Task> parameterAspectDelegate)
        {
            delegates.Add(next => (p, ctx) => parameterAspectDelegate(p, ctx, next));
        }

        private ParameterAspectDelegate Build()
        {
            ParameterAspectDelegate invoke = (p, ctx) => Completed;

            foreach (var next in delegates.Reverse())
            {
                invoke = next(invoke);
            }

            return invoke;
        }

        public Task Invoke(IParameterDescriptor parameter, ParameterAspectContext context)
        {
            return Build()(parameter, context);
        }
    }
}
