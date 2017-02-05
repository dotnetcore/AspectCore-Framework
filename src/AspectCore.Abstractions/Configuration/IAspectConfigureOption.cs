using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public interface IAspectConfigureOption<TOption> : IEnumerable<Func<MethodInfo, TOption>>
    {
        void Add(Func<MethodInfo, TOption> configure);
    }
}
