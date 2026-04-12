namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal abstract class ProxyBuilderNode
    {
        public abstract void Accept(IProxyBuilderVisitor visitor);
    }
}
