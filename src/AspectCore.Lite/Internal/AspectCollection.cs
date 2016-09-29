using AspectCore.Lite.Abstractions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectCollection : IAspectCollection
    {
        private readonly IList<Aspect> aspects = new List<Aspect>();
        public Aspect this[int index]
        {
            get
            {
                return aspects[index];
            }

            set
            {
                aspects[index] = value;
            }
        }

        public int Count => aspects.Count;

        public bool IsReadOnly { get; } = false;

        public void Add(Aspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            aspects.Add(aspect);
        }

        public void Clear()
        {
            aspects.Clear();
        }

        public bool Contains(Aspect aspect)
        {
            return aspects.Contains(aspect);
        }

        public void CopyTo(Aspect[] array, int arrayIndex)
        {
            aspects.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Aspect> GetEnumerator()
        {
            return aspects.GetEnumerator();
        }

        public int IndexOf(Aspect aspect)
        {
            return aspects.IndexOf(aspect);
        }

        public void Insert(int index, Aspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            aspects.Insert(index, aspect);
        }

        public bool Remove(Aspect aspect)
        {
            return aspects.Remove(aspect);
        }

        public void RemoveAt(int index)
        {
            aspects.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
