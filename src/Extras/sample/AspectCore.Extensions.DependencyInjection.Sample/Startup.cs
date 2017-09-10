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
            services.AddMvc();

            services.AddOptions();

            services.AddTransient<IValuesService, ValuesService>();

            #region 方式一：使用AspectCore.Injector内置的IoC容器
            //方式一：使用AspectCore.Injector内置的IoC容器
            //方式一步骤1.调用services.ToServiceContainer()得到AspectCore内置容器IServiceContainer
            var container = services.ToServiceContainer();

            //方式一步骤2.调用IServiceContainer.Configure配置全局拦截器
            container.Configure(config =>
            {
                config.Interceptors.AddTyped<ServiceExecuteLoggerInterceptor>(Predicates.ForService("*Service"));
            });

            //方式一步骤3.调用IServiceContainer.Build构建动态代理服务解析器
            return container.Build();
            #endregion

            #region 方式二：使用Microsoft.Extensions.DependencyInjection容器
            ////方式二：使用Microsoft.Extensions.DependencyInjection容器
            ////方式二步骤1.services.AddDynamicProxy添加动态代理服务和配置全局拦截器
            //services.AddDynamicProxy(config =>
            //{
            //    config.Interceptors.AddTyped<ServiceExecuteLoggerInterceptor>(Predicates.ForService("*Service"));
            //});
            ////方式二步骤2.调用services.BuildAspectCoreServiceProvider构建动态代理服务解析器
            //return services.BuildAspectCoreServiceProvider();
            #endregion

        }

        #region 方式三：使用ConfigureContainer和ServiceProviderFactory配置AspectCore.Injector内置的IoC容器
        /// <summary>
        /// 方式三：使用ConfigureContainer和ServiceProviderFactory配置AspectCore.Injector内置的IoC容器
        /// 方式三步骤1.添加ConfigureContainer方法进行容器和动态代理配置
        /// 方式三步骤2.在Program类BuildWebHost方法中，添加.ConfigureServices(services => services.AddAspectCoreContainer())
        /// </summary>
        /// <param name="serviceContainer"></param>
        //public void ConfigureContainer(IServiceContainer serviceContainer)
        //{
        //    serviceContainer.Configure(config =>
        //    {
        //        config.Interceptors.AddTyped<ServiceExecuteLoggerInterceptor>(Predicates.ForService("*Service"));
        //    });
        //}
        #endregion

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