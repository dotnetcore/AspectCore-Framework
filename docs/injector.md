# AspectCore中的IoC容器和依赖注入
IOC模式和依赖注入是近年来非常流行的一种模式，相信大家都不陌生了，在Asp.Net Core中提供了依赖注入作为内置的基础设施，如果仍不熟悉依赖注入的读者，可以看看由我们翻译的Asp.Net Core中文文档中依赖注入的相关章节: [ASP.NET Core 中文文档 第三章 原理（10）依赖注入](http://www.cnblogs.com/dotNETCoreSG/p/aspnetcore-3_10-dependency-injection.html)。基于IoC的重要性，AspectCore在提供Aop特性的同时，同样提供了可以和Aop无缝集成使用的轻量级、高性能IoC容器`AspectCore.Injector`。
### 开始使用
`AspectCore.Injector`内置在AspectCore.Core包中，我们可以通过nuget获取

```
   Install-Package AspectCore.Core -pre
```

### 容器和服务注册
在AspectCore.Injector中容器命名为`IServiceContainer`，使用容器的默认实现来创建一个容器，并且提供了类型，实例，和工厂三种方式来注册服务：

```
IServiceContainer services = new ServiceContainer();

//使用类型注册服务
services.AddType<ILogger, Logger>();

//使用实例注册服务，服务的生命周期限定为单例
services.AddInstance<ITaskService>(new TaskService());

//使用委托工厂注册服务
services.AddDelegate<ITaskService, TaskService>(resolver => new TaskService());
```

### 服务解析
AspectCore.Injector通过IServiceResolver来解析服务：

```
//创建服务解析器
IServiceResolver serviceResolver = services.Build();

//解析单个服务
ISampleService sampleService = serviceResolver.Resolve<ISampleService>();

//解析单个服务，并且验证是否为null，为null则抛出异常
ISampleService sampleServiceRequired = serviceResolver.ResolveRequired<ISampleService>();

//解析服务集合，如果未注册，则为空集合
IEnumerable<ISampleService> sampleServices = serviceResolver.ResolveMany<ISampleService>();

```

### 依赖注入
AspectCore.Injector提供构造器注入和属性两种方式：

```
public interface ISampleService
{
}

public class SampleService : ISampleService
{
    private readonly ISampleRepository _sampleRepository;
    private readonly ILogger _logger;
    
    //构造器注入
    public SampleService(ISampleRepository sampleRepository, ILogger logger)
    {
        _sampleRepository = sampleRepository;
        _logger = logger;
    }
}

public interface ISampleRepository
{
}

public class SampleRepository : ISampleRepository
{
    //属性注入。属性注入的条件为标记FromContainer特性，并且允许set。满足条件的属性自动注入
    [FromContainer]
    public ILogger Logger { get; set; }
}
```
### 生命周期
AspectCore.Injector提供以下生命周期：

瞬时  
瞬时（Transient）生命周期服务在它们每次请求时被创建。这一生命周期适合轻量级的，无状态的服务。

作用域  
作用域（Scoped）生命周期服务在每个作用域内被创建一次。

单例  
单例（Singleton）生命周期服务在它们第一次被解析时创建，并且每个后续解析将使用相同的实例。如果你的应用程序需要单例行为，建议让服务容器管理服务的生命周期而不是在自己的类中实现单例模式和管理对象的生命周期。

### Aop集成
在AspectCore.Injector中默认开启在AspectCore.DynamicProxy的Aop集成，并可通过IServiceContainer的Configure方法进行Aop的配置。

```
services.Configure(config =>
{
    config.Interceptors.AddTyped<SampleInterceptor>();
});

```

### 在Asp.Net Core中使用AspectCore.Injector
安装AspectCore.Extensions.DependencyInjection nuget包

```
    Install-Package AspectCore.Extensions.DependencyInjection -pre
```

在修改ConfigureServices：

```
public IServiceProvider ConfigureServices(IServiceCollection services)
{
    //添加你的服务...

    //将IServiceCollection的服务添加到ServiceContainer容器中
    var container = services.ToServiceContainer();
    return container.Build();
}
```

只需要两行代码即可在Asp.Net Core中使用AspectCore.Injector替换默认的DependencyInjection。


### 性能
Autofac是目前.net/.net core较为流行的IoC容器之一，我们把Autofac(4.6.2版本)作为性能对比测试目标。分别从解析简单对象，属性注入和构造器注入三个方面对比性能。  
Benchmark类编写如下：

```
[AllStatisticsColumn]
[MemoryDiagnoser]
public class Benchmarks
{
    private readonly IServiceResolver serviceResolver;
    private readonly IContainer container;

    public Benchmarks()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterType<Logger>().As<ILogger>().InstancePerDependency();
        containerBuilder.RegisterType<TaskService>().As<ITaskService>().InstancePerDependency();
        containerBuilder.RegisterType<SampleRepository>().As<ISampleRepository>().InstancePerDependen
        containerBuilder.RegisterType<SampleService2>().As<ISampleService>().InstancePerDependency();
        container = containerBuilder.Build();

        var serviceContainer = new ServiceContainer();
        serviceContainer.AddType<ILogger, Logger>(Lifetime.Transient);
        serviceContainer.AddType<ITaskService, TaskService>(Lifetime.Transient);
        serviceContainer.AddType<ISampleRepository, SampleRepository>(Lifetime.Transient);
        serviceContainer.AddType<ISampleService, SampleService2>(Lifetime.Transient);
        serviceResolver = serviceContainer.Build();
    }

    [Benchmark]
    public object Autofac_Sample_Resolve()
    {
        return container.Resolve<ITaskService>();
    }

    [Benchmark]
    public object AspectCore_Sample_Resolve()
    {
        return serviceResolver.Resolve<ITaskService>();
    }

    [Benchmark]
    public object Autofac_PropertyInjection()
    {
        return container.Resolve<ISampleRepository>();
    }

    [Benchmark]
    public object AspectCore_PropertyInjection()
    {
        return serviceResolver.Resolve<ISampleRepository>();
    }

    [Benchmark]
    public object Autofac_ConstructorInjection()
    {
        return container.Resolve<ISampleService>();
    }

    [Benchmark]
    public object AspectCore_ConstructorInjection()
    {
        return serviceResolver.Resolve<ISampleService>();
    }
}
```
使用Release模式运行Benchmark：

```
BenchmarkDotNet=v0.10.8, OS=Windows 10 Threshold 2 (10.0.10586)
Processor=Intel Core i5-4590 CPU 3.30GHz (Haswell), ProcessorCount=4
Frequency=3215206 Hz, Resolution=311.0221 ns, Timer=TSC
dotnet cli version=2.0.0
  [Host]     : .NET Core 4.6.00001.0, 64bit RyuJIT
  DefaultJob : .NET Core 4.6.00001.0, 64bit RyuJIT

 |                          Method |        Mean |         Min |         Max |         Op/s |  Gen 0 | Allocated |
 |-------------------------------- |------------:|------------:|------------:|-------------:|-------:|----------:|
 |          Autofac_Sample_Resolve |   494.83 ns |   482.52 ns |   506.58 ns |  2,020,908.9 | 0.2384 |     752 B |
 |       AspectCore_Sample_Resolve |    88.52 ns |    87.92 ns |    89.31 ns | 11,296,837.3 | 0.0279 |      88 B |
 |       Autofac_PropertyInjection | 2,014.46 ns | 2,004.18 ns | 2,028.83 ns |    496,411.0 | 0.5875 |    1856 B |
 |    AspectCore_PropertyInjection |   307.55 ns |   303.61 ns |   310.74 ns |  3,251,544.6 | 0.1063 |     336 B |
 |    Autofac_ConstructorInjection | 1,465.71 ns | 1,454.43 ns | 1,480.38 ns |    682,263.5 | 0.6084 |    1920 B |
 | AspectCore_ConstructorInjection |   284.94 ns |   283.55 ns |   286.05 ns |  3,509,500.8 | 0.0987 |     312 B |

```

Sample：[IoC-Sample](https://github.com/AspectCore/IoC-Sample)
