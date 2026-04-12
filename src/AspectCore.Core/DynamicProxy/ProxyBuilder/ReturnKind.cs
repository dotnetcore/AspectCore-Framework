namespace AspectCore.DynamicProxy.ProxyBuilder
{
    internal enum ReturnKind
    {
        Void,
        Sync,
        Task,
        TaskOfT,
        ValueTask,
        ValueTaskOfT
    }
}
