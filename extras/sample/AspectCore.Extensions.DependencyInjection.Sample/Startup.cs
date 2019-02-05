using System;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection.Sample.DynamicProxy;
using AspectCoreExtensions.DependencyInjection.Sample.Services;
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

            services.ConfigureDynamicProxy(config =>
            {
                config.Interceptors.AddTyped<MethodExecuteLoggerInterceptor>();
            });
            return services.BuildDynamicProxyServiceProvider();
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