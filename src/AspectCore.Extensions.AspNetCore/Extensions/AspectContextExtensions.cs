using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.DynamicProxy
{
    public static class AspectContextExtensions
    {
        /// <summary>
        /// 获取http上下文
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <returns>http上下文</returns>
        public static HttpContext GetHttpContext(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            var httpContextAccessor = aspectContext.ServiceProvider.GetService<IHttpContextAccessor>();
            return httpContextAccessor?.HttpContext;
        }
    }
}
