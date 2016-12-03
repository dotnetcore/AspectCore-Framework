using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Internals
{
    internal sealed class NamedMethodMatcher : INamedMethodMatcher
    {
        public MethodInfo Match(Type declaringType , string methodName , params object[] parameters)
        {
            ExceptionHelper.ThrowArgumentNull(declaringType , nameof(declaringType));
            ExceptionHelper.ThrowArgumentNull(parameters , nameof(parameters));
            ExceptionHelper.ThrowArgumentNullOrEmpty(methodName , nameof(methodName));

            var matchNameMethods = declaringType.GetTypeInfo().DeclaredMethods.Where(m => m.Name == methodName).ToArray();

            if (matchNameMethods.Length == 1)
            {
                return matchNameMethods[0];
            }

            var matchParameterMethods = matchNameMethods.Where(m => m.GetParameters().Length == parameters.Length).ToArray();

            if (matchParameterMethods.Length == 1)
            {
                return matchParameterMethods[0];
            }

            int bestLength = -1;
            var bestMatcher = default(MethodOfGivenParametersMatcher);

            var matchers = matchParameterMethods.Select(m => new MethodOfGivenParametersMatcher(m));

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

            ExceptionHelper.Throw<InvalidOperationException>(() => bestMatcher == null , 
                $"A suitable method for type '{declaringType}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public method.");

            return (MethodInfo)bestMatcher.GetMethod();
        }
    }
}
