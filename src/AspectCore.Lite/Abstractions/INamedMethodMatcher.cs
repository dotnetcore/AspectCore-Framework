using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    public interface INamedMethodMatcher
    {
        MethodInfo Match(Type type , string methodName , params object[] parameters);
    }
}
