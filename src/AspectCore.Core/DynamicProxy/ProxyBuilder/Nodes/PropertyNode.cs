using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class PropertyNode : ProxyBuilderNode
    {
        public string Name { get; }

        public Type PropertyType { get; }

        public PropertyAttributes PropertyAttributes { get; }

        public IReadOnlyList<AttributeNode> Attributes { get; }

        public MethodNode GetMethod { get; }

        public MethodNode SetMethod { get; }

        public FieldNode BackingField { get; }

        public PropertyNode(
            string name,
            Type propertyType,
            PropertyAttributes propertyAttributes,
            IReadOnlyList<AttributeNode> attributes,
            MethodNode getMethod,
            MethodNode setMethod,
            FieldNode backingField = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            PropertyAttributes = propertyAttributes;
            Attributes = attributes ?? Array.Empty<AttributeNode>();
            GetMethod = getMethod;
            SetMethod = setMethod;
            BackingField = backingField;
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitProperty(this);
    }
}
