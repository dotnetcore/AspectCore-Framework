using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;

namespace AspectCore.DynamicProxy.ProxyBuilder.Builders
{
    internal static class AttributeNodeFactory
    {
        public static AttributeNode CreateMarker(Type attributeType)
        {
            return new AttributeNode(attributeType);
        }

        public static List<AttributeNode> FromCustomAttributes(IEnumerable<CustomAttributeData> attributeDataList)
        {
            var result = new List<AttributeNode>();
            foreach (var data in attributeDataList)
            {
                result.Add(new AttributeNode(data));
            }
            return result;
        }
    }
}
