using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;

namespace AspectCore.DynamicProxy.ProxyBuilder.Builders
{
    internal static class ParameterNodeFactory
    {
        public static List<ParameterNode> FromMethod(MethodInfo method)
        {
            var result = new List<ParameterNode>();
            foreach (var param in method.GetParameters())
            {
                result.Add(FromParameterInfo(param));
            }
            // Return parameter
            result.Add(FromReturnParameter(method.ReturnParameter));
            return result;
        }

        public static List<ParameterNode> FromConstructor(ConstructorInfo constructor)
        {
            var result = new List<ParameterNode>();
            foreach (var param in constructor.GetParameters())
            {
                result.Add(FromParameterInfo(param));
            }
            return result;
        }

        private static ParameterNode FromParameterInfo(ParameterInfo param)
        {
            bool hasDefault = param.HasDefaultValueByAttributes();
            object defaultValue = null;

            if (hasDefault)
            {
                defaultValue = TryGetDefaultValue(param);
            }

            var attributes = new List<AttributeNode>
            {
                AttributeNodeFactory.CreateMarker(typeof(DynamicallyAttribute))
            };
            attributes.AddRange(AttributeNodeFactory.FromCustomAttributes(param.CustomAttributes));

            return new ParameterNode(
                param.Position,
                param.Name,
                param.ParameterType,
                param.Attributes,
                hasDefault,
                defaultValue,
                attributes);
        }

        private static ParameterNode FromReturnParameter(ParameterInfo param)
        {
            var attributes = new List<AttributeNode>
            {
                AttributeNodeFactory.CreateMarker(typeof(DynamicallyAttribute))
            };
            attributes.AddRange(AttributeNodeFactory.FromCustomAttributes(param.CustomAttributes));

            return new ParameterNode(
                -1,
                param.Name,
                param.ParameterType,
                param.Attributes,
                false,
                null,
                attributes);
        }

        private static object TryGetDefaultValue(ParameterInfo param)
        {
            try
            {
                return param.DefaultValue;
            }
            catch (FormatException) when (param.ParameterType == typeof(DateTime))
            {
                return null;
            }
            catch (FormatException) when (param.ParameterType.IsEnum)
            {
                return null;
            }
        }
    }
}
