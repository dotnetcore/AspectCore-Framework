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

        /// <summary>
        /// Gets a value indicating whether the property is declared as <c>partial</c>
        /// (C# 13.0 partial properties). When <c>true</c>, the property declaration
        /// and accessor implementations may be split across multiple source files.
        /// </summary>
        public bool IsPartial { get; }

        public PropertyNode(
            string name,
            Type propertyType,
            PropertyAttributes propertyAttributes,
            IReadOnlyList<AttributeNode> attributes,
            MethodNode getMethod,
            MethodNode setMethod,
            FieldNode backingField = null,
            bool isPartial = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            PropertyAttributes = propertyAttributes;
            Attributes = attributes ?? Array.Empty<AttributeNode>();
            GetMethod = getMethod;
            SetMethod = setMethod;
            BackingField = backingField;
            IsPartial = isPartial;
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitProperty(this);
    }
}
