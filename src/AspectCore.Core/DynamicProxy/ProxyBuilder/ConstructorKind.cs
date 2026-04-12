namespace AspectCore.DynamicProxy.ProxyBuilder
{
    internal enum ConstructorKind
    {
        DefaultObjectCtor,
        InterfaceProxyCtorWithFactory,
        InterfaceProxyCtorWithFactoryAndTarget,
        ClassProxyCtorFromBase
    }
}
