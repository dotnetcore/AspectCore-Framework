using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    internal class AspectCaching : IAspectCaching
    {
        private readonly ConcurrentDictionary<object, object> _dictionary;

        public AspectCaching(string name)
        {
            Name = name;
            _dictionary = new ConcurrentDictionary<object, object>();
        }

        public string Name { get; }

        public void Dispose()
        {
            foreach (var key in _dictionary.Keys.ToArray())
            {
                if (_dictionary.TryRemove(key, out var value))
                {
                    var enumerbale = value as IEnumerable;
                    if (enumerbale != null)
                    {
                        foreach (var item in enumerbale)
                        {
                            var d = item as IDisposable;
                            d?.Dispose();
                        }
                    }
                    var disposable = value as IDisposable;
                    disposable?.Dispose();
                }
            }
        }

        public object Get(object key)
        {
            return _dictionary[key];
        }

        public object GetOrAdd(object key, Func<object, object> factory)
        {
            return _dictionary.GetOrAdd(key, factory);
        }

        public void Set(object key, object value)
        {
            _dictionary[key] = value;
        }
    }
}