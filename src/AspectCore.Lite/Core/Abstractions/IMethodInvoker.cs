using AspectCore.Lite.Core.Descriptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public interface IMethodInvoker
    {
        object Invoke(ParameterCollection parameterCollection);
    }
}
