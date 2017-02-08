using AspectCore.Abstractions.Extensions;

namespace AspectCore.Abstractions.Extensions
{
    internal static class IgnoreConfigureExtensions
    {
        internal static IAspectConfigureOption<bool> IgnoreAspNetCore(this IAspectConfigureOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.AspNetCore.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.AspNet.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.Extensions.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.ApplicationInsights.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.Net.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.Web.*"));
            return option;
        }

        internal static IAspectConfigureOption<bool> IgnoreEntityFramework(this IAspectConfigureOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.Data.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.EntityFrameworkCore"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.EntityFrameworkCore.*"));
            return option;
        }

        internal static IAspectConfigureOption<bool> IgnoreOwin(this IAspectConfigureOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Matches("Microsoft.Owin.*"));
            option.Add(method => method.DeclaringType.Namespace.Matches("Owin"));
            return option;
        }

        internal static IAspectConfigureOption<bool> IgnorePageGenerator(this IAspectConfigureOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Matches("PageGenerator"));
            return option;
        }

        internal static IAspectConfigureOption<bool> IgnoreSystem(this IAspectConfigureOption<bool> option)
        {
            option.Add(method => method.DeclaringType.Namespace.Matches("System"));
            option.Add(method => method.DeclaringType.Namespace.Matches("System.*"));
            return option;
        }

        internal static IAspectConfigureOption<bool> IgnoreObjectVMethod(this IAspectConfigureOption<bool> option)
        {
            option.Add(method => method.Name.Matches("Equals"));
            option.Add(method => method.Name.Matches("GetHashCode"));
            option.Add(method => method.Name.Matches("ToString"));
            option.Add(method => method.Name.Matches("GetType"));
            return option;
        }
    }
}
