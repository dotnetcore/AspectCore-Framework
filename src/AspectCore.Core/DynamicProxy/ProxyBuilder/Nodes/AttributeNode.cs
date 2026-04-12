using System;
using System.Reflection;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class AttributeNode : ProxyBuilderNode
    {
        public CustomAttributeData CustomAttributeData { get; }

        public Type MarkerAttributeType { get; }

        public bool IsMarker => MarkerAttributeType != null;

        public AttributeNode(CustomAttributeData data)
        {
            CustomAttributeData = data ?? throw new ArgumentNullException(nameof(data));
        }

        public AttributeNode(Type markerType)
        {
            MarkerAttributeType = markerType ?? throw new ArgumentNullException(nameof(markerType));
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitAttribute(this);
    }
}
