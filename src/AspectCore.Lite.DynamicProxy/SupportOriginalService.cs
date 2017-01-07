using AspectCore.Lite.Abstractions.Resolution;
using System;

namespace AspectCore.Lite.DynamicProxy
{
    internal class SupportOriginalService : ISupportOriginalService
    {
        private readonly object instance;

        public SupportOriginalService(object instance)
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
