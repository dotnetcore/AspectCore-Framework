using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Extensions.Configuration
{
    public static class NonAspectsCollectionExtensions
    {
        public static ICollection<Func<MethodInfo, bool>> AddNamespace(this ICollection<Func<MethodInfo, bool>> collection, string nameSpace)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForNamespace(nameSpace));

            return collection;
        }

        public static ICollection<Func<MethodInfo, bool>> AddService(this ICollection<Func<MethodInfo, bool>> collection, string service)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForService(service));

            return collection;
        }

        public static ICollection<Func<MethodInfo, bool>> AddMethod(this ICollection<Func<MethodInfo, bool>> collection, string method)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForMethod(method));

            return collection;
        }

        public static ICollection<Func<MethodInfo, bool>> AddMethod(this ICollection<Func<MethodInfo, bool>> collection, string service, string method)
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
