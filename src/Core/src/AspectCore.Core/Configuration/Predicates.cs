using System;
using AspectCore.Abstractions;

namespace AspectCore.Core.Configuration
{
    public static class Predicates
    {
        public static AspectPredicate ForNamespace(string nameSpace)
        {
            if (nameSpace == null)
            {
                throw new ArgumentNullException(nameof(nameSpace));
            };

            return method => method.DeclaringType.Namespace.Matches(nameSpace);
        }

        public static AspectPredicate ForService(string service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            return method => method.DeclaringType.FullName.Matches(service);
        }

        public static AspectPredicate ForMethod(string method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return methodInfo => methodInfo.Name.Matches(method);
        }

        public static AspectPredicate ForMethod(string service, string method)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return methodInfo => methodInfo.DeclaringType.FullName.Matches(service) && methodInfo.Name.Matches(method);
        }
    }
}
