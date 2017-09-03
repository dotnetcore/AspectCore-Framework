using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Injector
{
    public sealed class ManyEnumerable<T> : IManyEnumerable<T>
    {
        private readonly T[] _array;

        public ManyEnumerable(T[] array)
        {
            _array = array;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _array)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _array.GetEnumerator();
        }
    }
}
