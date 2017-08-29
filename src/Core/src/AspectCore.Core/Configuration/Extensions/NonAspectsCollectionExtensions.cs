using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Core.Configuration
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

        internal static ICollection<Func<MethodInfo, bool>> AddAspNetCore(this ICollection<Func<MethodInfo, bool>> collection)
        {
            collection.AddNamespace("Microsoft.AspNetCore.*");
            collection.AddNamespace("Microsoft.AspNet.*");
            collection.AddNamespace("Microsoft.Extensions.*");
            collection.AddNamespace("Microsoft.ApplicationInsights.*");
            collection.AddNamespace("Microsoft.Net.*");
            collection.AddNamespace("Microsoft.Web.*");
            return collection;
        }

        internal static ICollection<Func<MethodInfo, bool>> AddEntityFramework(this ICollection<Func<MethodInfo, bool>> collection)
        {
            collection.AddNamespace("Microsoft.Data.*");
            collection.AddNamespace("Microsoft.EntityFrameworkCore");
            collection.AddNamespace("Microsoft.EntityFrameworkCore.*");
            return collection;
        }

        internal static ICollection<Func<MethodInfo, bool>> AddOwin(this ICollection<Func<MethodInfo, bool>> collection)
        {
            collection.AddNamespace("Microsoft.Owin.*");
            collection.AddNamespace("Owin");
            return collection;
        }

        internal static ICollection<Func<MethodInfo, bool>> AddPageGenerator(this ICollection<Func<MethodInfo, bool>> collection)
        {
            collection.AddNamespace("PageGenerator");
            return collection;
        }

        internal static ICollection<Func<MethodInfo, bool>> AddSystem(this ICollection<Func<MethodInfo, bool>> collection)
        {
            collection.AddNamespace("System");
            collection.AddNamespace("System.*");
            return collection;
        }

        internal static ICollection<Func<MethodInfo, bool>> AddObjectVMethod(this ICollection<Func<MethodInfo, bool>> collection)
        {
            collection.AddMethod("Equals");
            collection.AddMethod("GetHashCode");
            collection.AddMethod("ToString");
            collection.AddMethod("GetType");
            return collection;
        }

        internal static ICollection<Func<MethodInfo, bool>> AddDefault(this ICollection<Func<MethodInfo, bool>> collection)
        {
            return collection.
                AddObjectVMethod().
                AddSystem().
                AddAspNetCore().
                AddEntityFramework().
                AddOwin().
                AddPageGenerator();
        }
    }
}