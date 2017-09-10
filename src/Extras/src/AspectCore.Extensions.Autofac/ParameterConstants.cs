using Autofac.Core;
using Autofac.Core.Activators.Reflection;

namespace AspectCore.Extensions.Autofac
{
    internal class ParameterConstants
    {
        internal readonly static Parameter[] DefaultParameters = new Parameter[] { new AutowiringParameter(), new DefaultValueParameter() };
    }
}
