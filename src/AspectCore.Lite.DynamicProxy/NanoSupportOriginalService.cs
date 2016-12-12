using AspectCore.Lite.Abstractions.Resolution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.DynamicProxy
{
    internal class NanoSupportOriginalService : ISupportOriginalService
    {
        private readonly object instance;

        public NanoSupportOriginalService(object instance)
        {
            this.instance = instance;
        }
        public void Dispose()
        {
        }

        public object GetService(Type serviceType)
        {
            return instance;
        }
    }
}
