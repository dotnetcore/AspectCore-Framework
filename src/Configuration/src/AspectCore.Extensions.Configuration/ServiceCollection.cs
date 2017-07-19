using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.Configuration
{
    internal sealed class ServiceCollection : IServiceCollection
    {
        private readonly IList<ServiceDescriptor> _collection = new List<ServiceDescriptor>();

        public ServiceDescriptor this[int index] { get => _collection[index]; set => _collection[index] = value; }

        public int Count => _collection.Count;

        public bool IsReadOnly => _collection.IsReadOnly;

        public void Add(ServiceDescriptor item) => _collection.Add(item);

        public void Clear() => _collection.Clear();

        public bool Contains(ServiceDescriptor item) => _collection.Contains(item);

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => _collection.CopyTo(array, arrayIndex);

        public IEnumerator<ServiceDescriptor> GetEnumerator() => _collection.GetEnumerator();

        public int IndexOf(ServiceDescriptor item) => _collection.IndexOf(item);

        public void Insert(int index, ServiceDescriptor item) => _collection.Insert(index, item);

        public bool Remove(ServiceDescriptor item) => _collection.Remove(item);

        public void RemoveAt(int index) => _collection.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
    }
}
