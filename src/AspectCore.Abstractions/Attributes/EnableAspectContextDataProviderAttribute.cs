using AspectCore.Abstractions.Internal;
using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    public sealed class EnableAspectContextDataProviderAttribute : InterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            foreach(var parameter in context.Parameters)
            {
                if (parameter.ParameterType == typeof(IAspectContextDataProvider))
                {
                    parameter.Value = new AspectContextDataProvider(context.Items);
                }
            }
            return next(context);
        }
    }
}