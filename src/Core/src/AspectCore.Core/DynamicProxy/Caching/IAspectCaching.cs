using System;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    public interface IAspectCaching : IDisposable
    {
        string Name { get; }

        object Get(object key);

        void Set(object key, object value);

        object GetOrAdd(object key, Func<object, object> factory);
    }
}
