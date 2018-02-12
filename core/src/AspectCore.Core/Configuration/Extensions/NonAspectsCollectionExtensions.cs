using System;

namespace AspectCore.Configuration
{
    public static class NonAspectsCollectionExtensions
    {
        public static NonAspectPredicateCollection AddNamespace(this NonAspectPredicateCollection collection, string nameSpace)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForNameSpace(nameSpace));

            return collection;
        }

        public static NonAspectPredicateCollection AddService(this NonAspectPredicateCollection collection, string service)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForService(service));

            return collection;
        }

        public static NonAspectPredicateCollection AddMethod(this NonAspectPredicateCollection collection, string method)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForMethod(method));

            return collection;
        }

        public static NonAspectPredicateCollection AddMethod(this NonAspectPredicateCollection collection, string service, string method)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            collection.Add(Predicates.ForMethod(service, method));

            return collection;
        }

        private static NonAspectPredicateCollection AddMicrosoft(this NonAspectPredicateCollection collection)
        {        
            collection.AddNamespace("Microsoft.*");
            return collection;
        }

        private static NonAspectPredicateCollection AddCodeAnalysis(this NonAspectPredicateCollection collection)
        {
            collection.AddNamespace("Microsoft.CodeAnalysis.Razor");
            collection.AddNamespace("Microsoft.CodeAnalysis.Razor.*");
            return collection;
        }

        private static NonAspectPredicateCollection AddAspNetCore(this NonAspectPredicateCollection collection)
        {
            collection.AddNamespace("Microsoft.AspNetCore.*");
            collection.AddNamespace("Microsoft.AspNetCore.Razor.Language");
            collection.AddNamespace("Microsoft.AspNet.*");
            collection.AddNamespace("Microsoft.Extensions.*");
            collection.AddNamespace("Microsoft.ApplicationInsights.*");
            collection.AddNamespace("Microsoft.Net.*");
            collection.AddNamespace("Microsoft.Web.*");
            return collection;
        }

        private static NonAspectPredicateCollection AddEntityFramework(this NonAspectPredicateCollection collection)
        {
            collection.AddNamespace("Microsoft.Data.*");
            collection.AddNamespace("Microsoft.EntityFrameworkCore");
            collection.AddNamespace("Microsoft.EntityFrameworkCore.*");
            return collection;
        }

        private static NonAspectPredicateCollection AddOwin(this NonAspectPredicateCollection collection)
        {
            collection.AddNamespace("Microsoft.Owin.*");
            collection.AddNamespace("Owin");
            return collection;
        }

        private static NonAspectPredicateCollection AddPageGenerator(this NonAspectPredicateCollection collection)
        {
            collection.AddNamespace("PageGenerator");
            collection.AddNamespace("PageGenerator*");
            return collection;
        }

        private static NonAspectPredicateCollection AddSystem(this NonAspectPredicateCollection collection)
        {
            collection.AddNamespace("System");
            collection.AddNamespace("System.*");
            return collection;
        }

        private static NonAspectPredicateCollection AddObjectVMethod(this NonAspectPredicateCollection collection)
        {
            collection.AddMethod("Equals");
            collection.AddMethod("GetHashCode");
            collection.AddMethod("ToString");
            collection.AddMethod("GetType");
            collection.AddMethod("Finalize");
            collection.Add(m => m.DeclaringType == typeof(object));
            return collection;
        }

        private static NonAspectPredicateCollection AddAspectCore(this NonAspectPredicateCollection collection)
        {
            collection.AddNamespace("AspectCore.Configuration");
            collection.AddNamespace("AspectCore.DynamicProxy");
            collection.AddNamespace("AspectCore.Injector");
            collection.AddNamespace("AspectCore.Configuration.*");
            collection.AddNamespace("AspectCore.DynamicProxy.*");
            collection.AddNamespace("AspectCore.Injector.*");
            collection.AddNamespace("AspectCore.Extensions.*");
            return collection;
        }

        private static NonAspectPredicateCollection AddIdentityServer4(this NonAspectPredicateCollection collection)
        {
            collection.AddNamespace("IdentityServer4");
            collection.AddNamespace("IdentityServer4.*");
            return collection;
        }

        private static NonAspectPredicateCollection AddButterfly(this NonAspectPredicateCollection collection)
        {
            collection.AddNamespace("Butterfly");
            collection.AddNamespace("Butterfly.*");
            return collection;
        }

        internal static NonAspectPredicateCollection AddDefault(this NonAspectPredicateCollection collection)
        {
            return collection.
                AddAspectCore().
                AddObjectVMethod().
                AddSystem().
                AddMicrosoft().
                AddCodeAnalysis().
                AddAspNetCore().
                AddEntityFramework().
                AddOwin().
                AddPageGenerator().
                AddIdentityServer4().
                AddButterfly();
        }
    }
}