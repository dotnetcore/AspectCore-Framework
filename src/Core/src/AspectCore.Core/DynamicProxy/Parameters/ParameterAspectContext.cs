using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy.Parameters
{
    public sealed class ParameterAspectContext
    {
        public object Value { get; set; }

        public string Name { get; }

        public AspectContext AspectContext { get; }

        public ParameterReflector Reflector { get; }

        internal ParameterAspectContext(AspectContext aspectContext, ParameterReflector reflector, object value)
        {
            AspectContext = aspectContext;
            Value = value;
            Reflector = reflector;
            Name = reflector.Name;
        }
    }
}