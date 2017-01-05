using AspectCore.Lite.Abstractions.Common;

namespace AspectCore.Lite.Abstractions.Resolution.Common
{
    internal static class IgnoreConfigurationExtensions
    {
        internal static IConfigurationOption<bool> IgnoreAspNetCore(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Match("Microsoft.AspNetCore.*"));
            option.Add(method => method.DeclaringType.Namespace.Match("Microsoft.AspNet.*"));
            option.Add(method => method.DeclaringType.Namespace.Match("Microsoft.Extensions.*"));
            option.Add(method => method.DeclaringType.Namespace.Match("Microsoft.ApplicationInsights.*"));
            option.Add(method => method.DeclaringType.Namespace.Match("Microsoft.Net.*"));
            option.Add(method => method.DeclaringType.Namespace.Match("Microsoft.Web.*"));
            return option;
        }

        internal static IConfigurationOption<bool> IgnoreEntityFramework(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Match("Microsoft.Data.*"));
            option.Add(method => method.DeclaringType.Namespace.Match("Microsoft.EntityFrameworkCore"));
            option.Add(method => method.DeclaringType.Namespace.Match("Microsoft.EntityFrameworkCore.*"));
            return option;
        }

        internal static IConfigurationOption<bool> IgnoreOwin(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Match("Microsoft.Owin.*"));
            option.Add(method => method.DeclaringType.Namespace.Match("Owin"));
            return option;
        }

        internal static IConfigurationOption<bool> IgnorePageGenerator(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Match("PageGenerator"));
            return option;
        }

        internal static IConfigurationOption<bool> IgnoreSystem(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Match("System"));
            option.Add(method => method.DeclaringType.Namespace.Match("System.*"));
            return option;
        }

        internal static IConfigurationOption<bool> IgnoreObjectVMethod(this IConfigurationOption<bool> option)
        {
            option.Add(method => method.Name.Match("Equals"));
            option.Add(method => method.Name.Match("GetHashCode"));
            option.Add(method => method.Name.Match("ToString"));
            option.Add(method => method.Name.Match("GetType"));
            return option;
        }
    }
}
