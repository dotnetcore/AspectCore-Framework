using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Lite.Abstractions.Aspects;

namespace AspectCore.Lite.Abstractions
{
    public class AspectCollection : IAspectCollection
    {
        private readonly IList<IAspect> aspects = new List<IAspect>();
        public IAspect this[int index]
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

        public void Add(IAspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            aspects.Add(aspect);
        }

        public void Clear()
        {
            aspects.Clear();
        }

        public bool Contains(IAspect aspect)
        {
            return aspects.Contains(aspect);
        }

        public void CopyTo(IAspect[] array, int arrayIndex)
        {
            aspects.CopyTo(array, arrayIndex);
        }

        public IEnumerator<IAspect> GetEnumerator()
        {
            return aspects.GetEnumerator();
        }

        public int IndexOf(IAspect aspect)
        {
            return aspects.IndexOf(aspect);
        }

        public void Insert(int index, IAspect aspect)
        {
            if (aspect == null) throw new ArgumentNullException(nameof(aspect));
            aspects.Insert(index, aspect);
        }

        public bool Remove(IAspect aspect)
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
