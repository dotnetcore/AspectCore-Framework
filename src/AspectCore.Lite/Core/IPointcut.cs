using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public interface IPointcut
    {
        bool IsMatch(MethodInfo method);
    }
}
