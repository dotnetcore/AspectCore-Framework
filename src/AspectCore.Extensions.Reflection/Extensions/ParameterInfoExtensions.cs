using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public static class ParameterInfoExtensions
    {
        public static bool HasDefaultValueByAttributes(this ParameterInfo parameter)
        {
            // parameter.HasDefaultValue will throw a FormatException when parameter is DateTime type with default value
            return (parameter.Attributes & ParameterAttributes.HasDefault) != 0;
        }

        public static object DefaultValueSafely(this ParameterInfo parameter)
        {
            try
            {
                // parameter.DefaultValue will throw a FormatException when parameter is DateTime type with default value
                return parameter.DefaultValue;
            }
            catch
            {
                return null;
            }
        }
    }
}
