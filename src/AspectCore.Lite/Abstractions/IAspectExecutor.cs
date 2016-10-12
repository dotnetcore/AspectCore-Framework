using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    public interface IAspectExecutor
    {
        object ExecuteSynchronously(object targetInstance , object proxyInstance , Type serviceType , string method , params object[] args);

        Task<object> ExecuteAsync(object targetInstance , object proxyInstance , Type serviceType , string method , params object[] args);
    }
}
