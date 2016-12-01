using System;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface INamedMethodMatcher
    {
        MethodInfo Match(Type type , string methodName , params object[] parameters);
    }
}
