namespace AspectCore.DynamicProxy.ProxyBuilder
{
    internal enum ReturnKind
    {
        Void,
        Sync,
        Task,
        TaskOfT,
        ValueTask,
        ValueTaskOfT,
        AsyncEnumerable,
        // C# 7.0 ref / ref readonly return. The interceptor pipeline is value-based
        // (AspectContext.ReturnValue is an object), so the intercepted value is
        // materialised into a StrongBox<T> whose field address is returned by ref.
        // See docs/3.CSharp-Language-Features-AOP-Emit-Adaptation.md 6.6.
        RefSync
    }
}
