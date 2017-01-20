using System.Reflection;

namespace AspectCore.Abstractions.Extensions
{
    internal static class MemberInfoExtensions
    {
        internal static string GetFullName(this System.Reflection.MemberInfo member)
        {
            var declaringType = member.DeclaringType.GetTypeInfo();
            if (declaringType.IsInterface)
            {
                return $"{declaringType.Name}.{member.Name}".Replace('+', '.');
            }
            return member.Name;
        }
    }
}
