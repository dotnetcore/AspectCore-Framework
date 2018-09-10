using System;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection.Sample.DynamicProxy;
using AspectCore.Extensions.DependencyInjection.Sample.Services;
using AspectCore.Injector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddControllersAsServices();

            services.AddOptions();

            services.AddTransient<IValuesService, ValuesService>();

            //方式二：使用Microsoft.Extensions.DependencyInjection容器
            //方式二步骤1.services.AddDynamicProxy添加动态代理服务和配置全局拦截器
            services.AddDynamicProxy(config =>
            {
                config.Interceptors.AddTyped<MethodExecuteLoggerInterceptor>();
            });
            //方式二步骤2.调用services.BuildAspectCoreServiceProvider构建动态代理服务解析器
            return services.BuildAspectInjectorProvider();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}