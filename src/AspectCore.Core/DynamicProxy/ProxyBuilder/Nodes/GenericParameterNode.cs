using System;
using System.Reflection;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class GenericParameterNode : ProxyBuilderNode
    {
        public string Name { get; }

        public GenericParameterAttributes Constraints { get; }

        public Type BaseTypeConstraint { get; }

        public Type[] InterfaceConstraints { get; }

        public AttributeNode[] Attributes { get; }

        public GenericParameterNode(string name, GenericParameterAttributes constraints, Type baseTypeConstraint, Type[] interfaceConstraints, AttributeNode[] attributes)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Constraints = constraints;
            BaseTypeConstraint = baseTypeConstraint;
            InterfaceConstraints = interfaceConstraints ?? Array.Empty<Type>();
            Attributes = attributes ?? Array.Empty<AttributeNode>();
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitGenericParameter(this);
    }
}
