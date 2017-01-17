using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public interface IConfigurationOption<TOption> : IEnumerable<Func<MethodInfo, TOption>>
    {
        void Add(Func<MethodInfo, TOption> configuration);
    }
}
