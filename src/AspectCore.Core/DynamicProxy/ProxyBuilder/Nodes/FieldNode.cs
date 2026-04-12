using System;
using System.Reflection;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class FieldNode : ProxyBuilderNode
    {
        public string Name { get; }

        public Type FieldType { get; }

        public FieldAttributes Accessibility { get; }

        public FieldNode(string name, Type fieldType, FieldAttributes accessibility = FieldAttributes.Private)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            FieldType = fieldType ?? throw new ArgumentNullException(nameof(fieldType));
            Accessibility = accessibility;
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitField(this);
    }
}
