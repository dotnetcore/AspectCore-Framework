using AspectCore.Abstractions.Resolution;
using System;
using System.Reflection;

namespace AspectCore.Abstractions.Extensions
{
    internal static class PropertyInfoExtensions
    {
        public static bool IsAccessibility(this PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return (property.CanRead && AspectValidator.IsAccessibility(property.GetMethod)) || (property.CanWrite && AspectValidator.IsAccessibility(property.GetMethod));
        }
    }
}
