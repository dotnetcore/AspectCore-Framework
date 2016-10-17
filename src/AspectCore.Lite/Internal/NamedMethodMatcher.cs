using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal sealed class NamedMethodMatcher : INamedMethodMatcher
    {
        public MethodInfo Match(Type declaringType , string methodName , params object[] parameters)
        {
            
            int bestLength = -1;
            var bestMatcher = default(MethodOfGivenParametersMatcher);

            var matchers = declaringType.GetTypeInfo().DeclaredMethods.Where(m => m.Name == methodName && m.GetParameters().Length == parameters.Length).Select(m => new MethodOfGivenParametersMatcher(m)).ToArray();

            if (matchers.Length == 1)
            {
                return (MethodInfo)matchers[0].GetMethod();
            }

            foreach (var matcher in matchers)
            {
                var length = matcher.Match(parameters);
                if (length == -1)
                {
                    continue;
                }
                if (bestLength < length)
                {
                    bestLength = length;
                    bestMatcher = matcher;
                }
            }

            if (bestMatcher == null)
            {
                var message = $"A suitable method for type '{declaringType}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public method.";
                throw new InvalidOperationException(message);
            }

            return (MethodInfo)bestMatcher.GetMethod();
        }
    }
}
