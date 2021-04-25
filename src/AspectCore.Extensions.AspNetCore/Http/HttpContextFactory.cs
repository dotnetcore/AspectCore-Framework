using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace AspectCore.Extensions.AspNetCore
{
    /// <summary>
    /// http上下文工厂
    /// </summary>
    public class HttpContextFactory : IHttpContextFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly FormOptions _formOptions;

        /// <summary>
        /// 创建http上下文工厂
        /// </summary>
        /// <param name="formOptions">FormOptions</param>
        /// <param name="httpContextAccessor">提供当前的访问权限 HttpContext （如果可用）</param>
        public HttpContextFactory(IOptions<FormOptions> formOptions, IHttpContextAccessor httpContextAccessor)
        {
            if (formOptions == null)
            {
                throw new ArgumentNullException(nameof(formOptions));
            }

            _formOptions = formOptions.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 通过IFeatureCollection创建http上下文
        /// </summary>
        /// <param name="IFeatureCollection">表示 HTTP 功能的集合</param>
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