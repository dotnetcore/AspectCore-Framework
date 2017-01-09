using AspectCore.Lite.Abstractions.Common;

namespace AspectCore.Lite.Abstractions.Resolution.Common
{
    internal static class IgnoreConfigurationExtensions
    {
        internal static IConfigurationOption<bool> IgnoreAspNetCore(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.AspNetCore.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.AspNet.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.Extensions.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.ApplicationInsights.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.Net.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.Web.*"));
            return option;
        }

        internal static IConfigurationOption<bool> IgnoreEntityFramework(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.Data.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.EntityFrameworkCore"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.EntityFrameworkCore.*"));
            return option;
        }

        internal static IConfigurationOption<bool> IgnoreOwin(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.Owin.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Owin"));
            return option;
        }

        internal static IConfigurationOption<bool> IgnorePageGenerator(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Matches("PageGenerator"));
            return option;
        }

        internal static IConfigurationOption<bool> IgnoreSystem(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Matches("System"));
            option.Add(method => method.DeclaringType.Namespace.Matches("System.*"));
            return option;
        }

        internal static IConfigurationOption<bool> IgnoreObjectVMethod(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.Name.Matches("Equals"));
            option.Add(method => method.Name.Matches("GetHashCode"));
            option.Add(method => method.Name.Matches("ToString"));
            option.Add(method => method.Name.Matches("GetType"));
            return option;
        }
    }
}
