using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class MethodReflector
    {
        private readonly string _displayName;

        public override string DisplayName => _displayName;

        private static string GetDisplayName(MethodInfo method)
        {
            var name = $"{method.ReturnType.GetReflector().DisplayName} {method.Name}";
            if (method.IsGenericMethod)
            {
                name += "<";
                var arguments = method.GetGenericArguments();
                name += arguments[0].GetReflector().DisplayName;
                for (var i = 1; i < arguments.Length; i++)
                {
                    name += ("," + arguments[0].GetReflector().DisplayName);
                }
                name += ">";
            }
            var parameterTypes = method.GetParameterTypes();
            name += "(";
            if (parameterTypes.Length == 0)
            {
                name += ")";
                return name;
            } 
            name += parameterTypes[0].GetReflector().DisplayName;
            for (var i = 1; i < parameterTypes.Length; i++)
            {
                name += ("," + parameterTypes[i].GetReflector().DisplayName);
            }
            name += ")";
            return name;
        }
    }
}