using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Injector;
using AspectCore.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Extensions.DependencyInjection.Sample.Web.DynamicProxy;
using AspectCore.Extensions.DependencyInjection.Sample.Web.Services;

namespace AspectCore.Extensions.DependencyInjection.Sample.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddTransient<IHomeService, HomeService>();

            services.AddDynamicProxy(config =>
            {
               config.Interceptors.AddTyped<ServiceExecuteLoggerInterceptor>(Predicates.ForNameSpace("AspectCore.Extensions.DependencyInjection.Sample.Web.Services"));
            });
            //return services.ToServiceContainer().Build();

            return services.BuildAspectCoreServiceProvider();
        }

        //public void ConfigureContainer(IServiceContainer serviceContainer)
        //{
        //    serviceContainer.Configuration.Interceptors.AddTyped<ActionExecuteLoggerInterceptor>(Predicates.ForService("*Controller"));
        //}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseMvcWithDefaultRoute();
        }
    }
}
