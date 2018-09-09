using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Injector;
using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using ZKWebStandard.Ioc;
using AutoFacIContainer = Autofac.IContainer;
using DryIocContainer = DryIoc.Container;
using ZKWebContainer = ZKWebStandard.Ioc.Container;

namespace AspectCore.IoC.Benchmarks
{
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class SimpleObjectBenchmarks
    {
        private readonly IServiceResolver serviceResolver;
        private readonly ServiceProvider serviceProvider;
        private readonly AutoFacIContainer autofacContainer;
        private readonly ZKWebContainer zkWebContainer;
        private readonly DryIocContainer dryIocContainer;

        public SimpleObjectBenchmarks()
        {
            var serviceContainer = new ServiceContainer();
            serviceContainer.Transients.AddType<IUserService, UserService1>();
            serviceContainer.Transients.AddType<IUserService, UserService2>();
            serviceContainer.Transients.AddType<IUserService, UserService>();
            serviceContainer.Transients.AddType(typeof(IRepository<>), typeof(Repository<>));
            serviceResolver = serviceContainer.Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IUserService, UserService1>();
            serviceCollection.AddTransient<IUserService, UserService2>();
            serviceCollection.AddTransient<IUserService, UserService>();
            serviceCollection.AddTransient(typeof(IRepository<>), typeof(Repository<>));
            serviceProvider = serviceCollection.BuildServiceProvider();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<UserService1>().As<IUserService>().InstancePerDependency();
            containerBuilder.RegisterType<UserService2>().As<IUserService>().InstancePerDependency();
            containerBuilder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>));
            containerBuilder.RegisterType<UserService>().As<IUserService>().InstancePerDependency();
            autofacContainer = containerBuilder.Build();

            zkWebContainer = new ZKWebContainer();
            zkWebContainer.Register<IUserService, UserService1>(ReuseType.Transient, null);
            zkWebContainer.Register<IUserService, UserService2>(ReuseType.Transient, null);
            zkWebContainer.Register<IUserService, UserService>(ReuseType.Transient, null);
            zkWebContainer.Register(typeof(IRepository<>), typeof(Repository<>), ReuseType.Transient, null);

            dryIocContainer = new DryIocContainer();
            dryIocContainer.Register<IUserService, UserService1>();
            dryIocContainer.Register<IUserService, UserService2>();
            dryIocContainer.Register<IUserService, UserService>();
            dryIocContainer.Register(typeof(IRepository<>), typeof(Repository<>));
        }

        [Benchmark]
        public IUserService AspectCoreIoC()
        {
            return serviceResolver.Resolve<IUserService>();
        }

        [Benchmark]
        public IUserService MicrosoftDependencyInjection()
        {
            return serviceProvider.GetService<IUserService>();
        }

        [Benchmark]
        public IUserService Autofac()
        {
            return autofacContainer.Resolve<IUserService>();
        }

        [Benchmark]
        public IUserService ZkWebIoC()
        {
            return zkWebContainer.Resolve<IUserService>(ZKWebStandard.Ioc.IfUnresolved.Throw, null);
        }


        [Benchmark]
        public IUserService DryIoC()
        {
            return dryIocContainer.Resolve<IUserService>();
        }

        //[Benchmark]
        public IUserService New()
        {
            //return new UserService(new Repository<User>());
            return null;
        }

        [Benchmark]
        public IEnumerable AspectCoreIoC_Enumerable()
        {
            return serviceResolver.Resolve<IEnumerable<IUserService>>().ToArray();
        }

        [Benchmark]
        public IEnumerable MicrosoftDependencyInjection_Enumerable()
        {
            return serviceProvider.GetServices<IUserService>().ToArray(); ;
        }

        [Benchmark]
        public IEnumerable Autofac_Enumerable()
        {
            return autofacContainer.Resolve<IEnumerable<IUserService>>().ToArray();
        }

        [Benchmark]
        public IEnumerable ZkWebIoC_Enumerable()
        {
            return zkWebContainer.ResolveMany<IUserService>(null).ToArray();
        }

        [Benchmark]
        public IEnumerable DryIoC_Enumerable()
        {
            return dryIocContainer.ResolveMany<IUserService>().ToArray();
        }
    }

    public interface IUserService { }

    public class UserService : IUserService
    {
        //public UserService(IRepository<User> repository) { }
    }

    public class UserService1 : IUserService
    {
    }

    public class UserService2 : IUserService
    {
    }

    public interface IRepository<T> { }

    public class Repository<T> : IRepository<T> { }

    public class User { }
}