using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal class ServiceScopeFactory:IServiceScopeFactory
    {
        private readonly ISupportOriginalService supportOriginalService;

        public ServiceScopeFactory(ISupportOriginalService supportOriginalService)
        {
            this.supportOriginalService = supportOriginalService;
        }

        public IServiceScope CreateScope()
        {
            return  new ServiceScope(supportOriginalService);
        }
    }
}