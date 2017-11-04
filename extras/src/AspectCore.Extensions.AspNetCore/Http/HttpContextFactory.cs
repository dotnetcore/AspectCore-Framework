using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace AspectCore.Extensions.AspNetCore
{
    public class HttpContextFactory : IHttpContextFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FormOptions _formOptions;

        public HttpContextFactory(IOptions<FormOptions> formOptions, IHttpContextAccessor httpContextAccessor)
        {
            if (formOptions == null)
            {
                throw new ArgumentNullException(nameof(formOptions));
            }

            _formOptions = formOptions.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public HttpContext Create(IFeatureCollection featureCollection)
        {
            if (featureCollection == null)
            {
                throw new ArgumentNullException(nameof(featureCollection));
            }

            var httpContext = new DefaultHttpContext(featureCollection);
            if (_httpContextAccessor != null)
            {
                _httpContextAccessor.HttpContext = httpContext;
            }

            var formFeature = new FormFeature(httpContext.Request, _formOptions);
            featureCollection.Set<IFormFeature>(formFeature);

            return httpContext;
        }

        public void Dispose(HttpContext httpContext)
        {
            if (_httpContextAccessor != null)
            {
                _httpContextAccessor.HttpContext = null;
            }
        }
    }
}