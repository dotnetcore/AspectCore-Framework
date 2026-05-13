using System;
using System.Collections.Generic;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class ProxyTypeNode : ProxyBuilderNode
    {
        public string Name { get; }

        public ProxyKind ProxyKind { get; }

        public Type ServiceType { get; }

        public Type ParentType { get; }

        public Type[] Interfaces { get; }

        public IReadOnlyList<GenericParameterNode> GenericParameters { get; }

        public IReadOnlyList<AttributeNode> Attributes { get; }

        public IReadOnlyList<FieldNode> Fields { get; }

        public IReadOnlyList<ConstructorNode> Constructors { get; }

        public IReadOnlyList<MethodNode> Methods { get; }

        public IReadOnlyList<PropertyNode> Properties { get; }

        public IReadOnlyList<MethodConstantNode> MethodConstants { get; }

        public ProxyTypeNode(
            string name,
            ProxyKind proxyKind,
            Type serviceType,
            Type parentType,
            Type[] interfaces,
            IReadOnlyList<GenericParameterNode> genericParameters,
            IReadOnlyList<AttributeNode> attributes,
            IReadOnlyList<FieldNode> fields,
            IReadOnlyList<ConstructorNode> constructors,
            IReadOnlyList<MethodNode> methods,
            IReadOnlyList<PropertyNode> properties,
            IReadOnlyList<MethodConstantNode> methodConstants)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ProxyKind = proxyKind;
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ParentType = parentType ?? throw new ArgumentNullException(nameof(parentType));
            Interfaces = interfaces ?? Type.EmptyTypes;
            GenericParameters = genericParameters ?? Array.Empty<GenericParameterNode>();
            Attributes = attributes ?? Array.Empty<AttributeNode>();
            Fields = fields ?? Array.Empty<FieldNode>();
            Constructors = constructors ?? Array.Empty<ConstructorNode>();
            Methods = methods ?? Array.Empty<MethodNode>();
            Properties = properties ?? Array.Empty<PropertyNode>();
            MethodConstants = methodConstants ?? Array.Empty<MethodConstantNode>();
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitProxyType(this);
    }
}
