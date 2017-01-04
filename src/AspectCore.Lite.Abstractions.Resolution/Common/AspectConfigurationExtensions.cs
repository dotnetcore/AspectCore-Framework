using AspectCore.Lite.Abstractions.Common;

namespace AspectCore.Lite.Abstractions.Resolution.Common
{
    internal static class AspectConfigurationExtensions
    {
        internal static IAspectConfiguration IgnoreAspNetCore(this IAspectConfiguration configuration)
        {
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("Microsoft.AspNetCore.*"));
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("Microsoft.AspNet.*"));
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("Microsoft.Extensions.*"));
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("Microsoft.ApplicationInsights.*"));
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("Microsoft.Net.*"));
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("Microsoft.Web.*"));
            return configuration;
        }

        internal static IAspectConfiguration IgnoreOwin(this IAspectConfiguration configuration)
        {
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("Microsoft.Owin.*"));
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("Owin"));
            return configuration;
        }

        internal static IAspectConfiguration IgnorePageGenerator(this IAspectConfiguration configuration)
        {
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("PageGenerator"));
            return configuration;
        }

        internal static IAspectConfiguration IgnoreSystem(this IAspectConfiguration configuration)
        {
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("System"));
            configuration.Ignore(method => method.DeclaringType.Namespace.Match("System.*"));
            return configuration;
        }

        internal static IAspectConfiguration IgnoreObjectVMethod(this IAspectConfiguration configuration)
        {
            configuration.Ignore(method => method.Name.Match("Equals"));
            configuration.Ignore(method => method.Name.Match("GetHashCode"));
            configuration.Ignore(method => method.Name.Match("ToString"));
            configuration.Ignore(method => method.Name.Match("GetType"));
            return configuration;
        }
    }
}
