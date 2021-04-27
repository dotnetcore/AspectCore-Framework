using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// 方法反射操作
    /// </summary>
    public partial class MethodReflector
    {
        private readonly string _displayName;

        /// <summary>
        /// 方法的显示名称
        /// </summary>
        public override string DisplayName => _displayName;

        /// <summary>
        /// 获取方法的显示名称
        /// </summary>
        /// <param name="method">方法</param>
        /// <returns>显示名称</returns>
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
                    name += ("," + arguments[i].GetReflector().DisplayName);
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
