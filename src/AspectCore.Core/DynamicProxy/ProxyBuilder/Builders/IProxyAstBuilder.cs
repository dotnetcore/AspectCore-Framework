using AspectCore.DynamicProxy.ProxyBuilder.Nodes;

namespace AspectCore.DynamicProxy.ProxyBuilder.Builders
{
    internal interface IProxyTypeBuilder
    {
        ProxyTypeNode[] Build();
    }
}
