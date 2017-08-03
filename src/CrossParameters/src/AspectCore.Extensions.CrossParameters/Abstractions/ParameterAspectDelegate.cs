using System.Threading.Tasks;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.CrossParameters
{
    public delegate Task ParameterAspectDelegate(IParameterDescriptor parameter , ParameterAspectContext context);
}
