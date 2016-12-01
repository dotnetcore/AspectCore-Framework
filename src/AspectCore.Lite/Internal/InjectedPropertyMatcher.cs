using AspectCore.Lite.Abstractions;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal sealed class InjectedPropertyMatcher : IInjectedPropertyMatcher
    {
        public PropertyInfo[] Match(IInterceptor interceptor)
        {
            return interceptor.GetType().GetTypeInfo().DeclaredProperties.Where(x => x.IsDefined(typeof(FromServiceAttribute))).ToArray();
        }
    }
}
