using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class ParameterNode : ProxyBuilderNode
    {
        public int Position { get; }

        public string Name { get; }

        public Type ParameterType { get; }

        public ParameterAttributes Attributes { get; }

        public object DefaultValue { get; }

        public bool HasDefaultValue { get; }

        public IReadOnlyList<AttributeNode> CustomAttributes { get; }

        public ParameterNode(int position, string name, Type parameterType, ParameterAttributes attributes,
            bool hasDefaultValue, object defaultValue, IReadOnlyList<AttributeNode> customAttributes)
        {
            Position = position;
            Name = name;
            ParameterType = parameterType ?? throw new ArgumentNullException(nameof(parameterType));
            Attributes = attributes;
            HasDefaultValue = hasDefaultValue;
            DefaultValue = defaultValue;
            CustomAttributes = customAttributes ?? Array.Empty<AttributeNode>();
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitParameter(this);
    }
}
