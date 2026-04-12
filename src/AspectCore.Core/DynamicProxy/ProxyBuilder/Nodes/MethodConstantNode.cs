using System;
using System.Reflection;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class MethodConstantNode : ProxyBuilderNode
    {
        public string Key { get; }

        public MethodInfo Method { get; }

        public MethodConstantNode(string key, MethodInfo method)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Method = method;
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitMethodConstant(this);
    }
}
