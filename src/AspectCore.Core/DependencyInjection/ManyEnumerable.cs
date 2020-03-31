using System.Collections;
using System.Collections.Generic;

namespace AspectCore.DependencyInjection
{
    public sealed class ManyEnumerable<T> : IManyEnumerable<T>
    {
        private readonly IEnumerable<T> _array;

        public ManyEnumerable(IEnumerable<T> array)
        {
            _array = array;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _array.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _array.GetEnumerator();
        }
    }
}
