using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;

namespace AspectCore.DynamicProxy.ProxyBuilder.Builders
{
    internal static class AttributeNodeFactory
    {
        // Full names of compiler-generated and AspectCore marker attributes that
        // must NOT be copied onto proxy types. Using string comparison (rather than
        // typeof()) because some of these attributes do not exist in every target
        // framework (e.g. netstandard2.0) and would fail to resolve at compile time.
        private static readonly HashSet<string> SkippedAttributeFullNames = new HashSet<string>
        {
            "System.Runtime.CompilerServices.NullableContextAttribute",
            "System.Runtime.CompilerServices.NullableAttribute",
            "System.Runtime.CompilerServices.CompilerGeneratedAttribute",
            "System.Runtime.CompilerServices.IsReadOnlyAttribute",
            "System.Runtime.CompilerServices.IsByRefLikeAttribute",
            "System.Runtime.CompilerServices.IsExternalInitAttribute",
            "System.Runtime.CompilerServices.PreserveBaseOverridesAttribute",
            "System.Runtime.CompilerServices.PrimaryConstructorParametersAttribute",
            "AspectCore.DynamicProxy.NonAspectAttribute",
            "AspectCore.DynamicProxy.DynamicallyAttribute"
        };

        public static AttributeNode CreateMarker(Type attributeType)
        {
            return new AttributeNode(attributeType);
        }

        public static List<AttributeNode> FromCustomAttributes(IEnumerable<CustomAttributeData> attributeDataList)
        {
            var result = new List<AttributeNode>();
            foreach (var data in attributeDataList)
            {
                // Skip compiler-generated attributes and AspectCore marker attributes
                // to avoid duplication and runtime issues on the generated proxy type.
                if (data.AttributeType != null &&
                    SkippedAttributeFullNames.Contains(data.AttributeType.FullName))
                {
                    continue;
                }

                result.Add(new AttributeNode(data));
            }
            return result;
        }
    }
}
