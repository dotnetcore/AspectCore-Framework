using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Resolution.Test.Fakes
{
    public class InstanceSupportOriginalService: ISupportOriginalService
    {
        private readonly object instance;

        public void Dispose()
        {
            var disposable = instance as IDisposable;
            disposable?.Dispose();
        }

        public object GetService(Type serviceType)
        {
            return instance;
        }
    }
}
