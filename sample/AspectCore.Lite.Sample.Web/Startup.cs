using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Aspects;
using System.Reflection;

namespace AspectCore.Lite.Sample.Web
{

    public static class AspectFactoryExtensions
    {
        public static Aspect CreateLoggingAspect(this IAspectFactory factory)
        {
            return factory.Create(new LoggingAdvice(), new LoggingPointCut());
        }
    }
    public class LoggingAdvice : IAdvice
    {
        public async Task ExecuteAsync(AspectContext aspectContext, AspectDelegate next)
        {
            var loggerFactory = aspectContext.ApplicationServices.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger(aspectContext.Target.GetTargetTypeInfo().AsType());
            logger.LogInformation("begin query");
            await next(aspectContext);
            logger.LogInformation("end query");
        }
    }

    public class LoggingPointCut : IPointcut
    {
        public bool IsMatch(MethodInfo methodInfo)
        {
            return methodInfo.Name.StartsWith("Query") && methodInfo.IsVirtual;
        }
    }


    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddMvc();

            services.AddAspects((aspects, factory) =>
            {
                aspects.Add(factory.CreateLoggingAspect());

            });


            return services.BuildAspectServiceProvider();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
