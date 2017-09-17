namespace AspectCore.DynamicProxy.Parameters
{
    public struct ParameterAspectContext
    {
        public Parameter Parameter { get; }

        public AspectContext AspectContext { get; }

        public ParameterAspectContext(AspectContext aspectContext, Parameter parameter)
        {
            AspectContext = aspectContext;
            Parameter = parameter;
        }
    }
}