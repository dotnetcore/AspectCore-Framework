using System;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.Configuration
{
    public static class NonAspectOptionCollectionExtensions
    {
        public static NonAspectOptionCollection Add(this NonAspectOptionCollection collection, Predicate<MethodInfo> predicate)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(new NonAspectOptions(predicate));

            return collection;
        }

        public static NonAspectOptionCollection AddNamespace(this NonAspectOptionCollection collection, string nameSpace)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForNamespace(nameSpace));

            return collection;
        }

        public static NonAspectOptionCollection AddService(this NonAspectOptionCollection collection, string service)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForService(service));

            return collection;
        }

        public static NonAspectOptionCollection AddMethod(this NonAspectOptionCollection collection, string method)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForMethod(method));

            return collection;
        }

        public static NonAspectOptionCollection AddMethod(this NonAspectOptionCollection collection, string service, string method)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForMethod(service, method));

            return collection;
        }
    }
}
