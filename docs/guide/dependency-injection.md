# 依赖注入集成

AspectCore 在提供 AOP 的同时，内置了一个轻量、高性能、可与动态代理无缝集成的 IoC 容器。你既可以直接使用这个内置容器 `IServiceContext`，也可以让它接管 `Microsoft.Extensions.DependencyInjection`，在 ASP.NET Core 中替换默认容器。本页覆盖两种用法：注册、解析、生命周期、构造器与属性注入、AOP 集成，以及一段与 Autofac 的性能对比。

## 内置容器 IServiceContext

内置容器的接口是 `IServiceContext`（命名空间 `AspectCore.DependencyInjection`），默认实现为 `ServiceContext`。它随 `AspectCore.Core` 提供。

### 服务注册

`IServiceContext` 通过扩展方法提供类型、实例、委托三种注册方式：

```csharp
using AspectCore.DependencyInjection;

IServiceContext services = new ServiceContext();

// 按类型注册
services.AddType<ILogger, Logger>();

// 按实例注册（生命周期为单例）
services.AddInstance<ITaskService>(new TaskService());

// 按委托工厂注册（委托入参是 IServiceResolver）
services.AddDelegate<ITaskService, TaskService>(resolver => new TaskService());
```

`AddType` / `AddDelegate` 都带可选的 `Lifetime` 参数（默认 `Transient`）；`AddInstance` 注册的服务固定是单例。

### 服务解析

先用 `Build()` 把 `IServiceContext` 构建成 `IServiceResolver`，再解析服务：

```csharp
// 构建解析器
IServiceResolver resolver = services.Build();

// 解析单个服务
ISampleService sampleService = resolver.Resolve<ISampleService>();

// 解析单个服务，未注册时抛异常
ISampleService required = resolver.ResolveRequired<ISampleService>();

// 解析服务集合，未注册时返回空集合
IEnumerable<ISampleService> many = resolver.ResolveMany<ISampleService>();
```

### 生命周期

`Lifetime` 枚举提供三种生命周期：

- **瞬时（Transient）**：每次请求都创建新实例，适合轻量、无状态的服务。
- **作用域（Scoped）**：每个作用域内创建一次。
- **单例（Singleton）**：首次解析时创建，之后所有解析复用同一实例。

```csharp
services.AddType<ILogger, Logger>(Lifetime.Singleton);
services.AddType<ITaskService, TaskService>(Lifetime.Scoped);
services.AddType<ISampleService, SampleService>(Lifetime.Transient);
```

### 构造器注入与属性注入

内置容器支持构造器注入和属性注入两种方式：

```csharp
using AspectCore.DependencyInjection;

public class SampleService : ISampleService
{
    private readonly ISampleRepository _repository;
    private readonly ILogger _logger;

    // 构造器注入
    public SampleService(ISampleRepository repository, ILogger logger)
    {
        _repository = repository;
        _logger = logger;
    }
}

public class SampleRepository : ISampleRepository
{
    // 属性注入：标记 [FromServiceContext] 且允许 set 的属性会被自动注入
    [FromServiceContext]
    public ILogger Logger { get; set; }
}
```

属性注入的条件是：属性标记 `[FromServiceContext]`（命名空间 `AspectCore.DependencyInjection`）且可写。

### AOP 集成

内置容器默认开启与 DynamicProxy 的 AOP 集成，通过 `Configure` 方法配置拦截器——用的是与其他宿主一致的 `IAspectConfiguration`：

```csharp
services.Configure(config =>
{
    config.Interceptors.AddTyped<SampleInterceptorAttribute>();
});
```

因此从容器解析出来的实例，如果命中了拦截器配置，就已经是织入切面的代理。拦截器配置的完整说明见[拦截器配置](./interceptor-configuration.md)。

## 与 Microsoft.Extensions.DependencyInjection 集成

多数应用不会直接用 `ServiceContext`，而是让 AspectCore 接管 `IServiceCollection`。安装 `AspectCore.Extensions.DependencyInjection`：

```bash
dotnet add package AspectCore.Extensions.DependencyInjection
```

### 配置拦截器：ConfigureDynamicProxy

在 `IServiceCollection` 上调用一次 `ConfigureDynamicProxy(...)` 启用动态代理并注册全局拦截器：

```csharp
using AspectCore.Extensions.DependencyInjection;

services.AddTransient<ICustomService, CustomService>();
services.ConfigureDynamicProxy(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>();
});
```

### 构建容器的两种方式

**方式一：`DynamicProxyServiceProviderFactory`（ASP.NET Core / 泛型主机推荐）**

在主机层用 `IServiceProviderFactory` 接管容器构建，业务代码无感：

```csharp
using AspectCore.Extensions.DependencyInjection;

Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new DynamicProxyServiceProviderFactory())
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
```

**方式二：`BuildDynamicProxyProvider` / `BuildServiceContextProvider`（手动构建）**

控制台或测试场景下直接构建 `IServiceProvider`：

```csharp
// 直接得到已启用动态代理的 ServiceProvider
var provider = services.BuildDynamicProxyProvider();

// 或者把 IServiceCollection 桥接到内置 ServiceContext 再构建
var provider2 = services.BuildServiceContextProvider();
```

### 在 ASP.NET Core 中使用

在 `Program`/`Startup` 中挂上 `DynamicProxyServiceProviderFactory`，并在 `ConfigureServices` 里照常注册服务、调用一次 `ConfigureDynamicProxy()`，即可用 AspectCore 的容器替换默认 DI。完整流程见[快速上手](../getting-started/quick-start.md)。

## 性能

内置容器在设计上追求低开销。官方以 Autofac（4.6.2）为对比，从解析简单对象、属性注入、构造器注入三方面做基准测试。以下数据来自历史基准（.NET Core 2.0，BenchmarkDotNet v0.10.8），仅供了解量级，实际数字随运行环境而变：

| Method | Mean | Allocated |
|--------|------|-----------|
| Autofac_Sample_Resolve | 494.83 ns | 752 B |
| AspectCore_Sample_Resolve | 88.52 ns | 88 B |
| Autofac_PropertyInjection | 2,014.46 ns | 1856 B |
| AspectCore_PropertyInjection | 307.55 ns | 336 B |
| Autofac_ConstructorInjection | 1,465.71 ns | 1920 B |
| AspectCore_ConstructorInjection | 284.94 ns | 312 B |

> 这组数据是历史环境下的旧基准，用于说明内置容器相对轻量，不代表当前版本或你的运行环境的确切表现。需要最新数据请在自己的环境重跑基准。

## 下一步

- [拦截器配置](./interceptor-configuration.md) — 全局拦截器的三种注册方式。
- [第三方容器](./third-party-containers.md) — 用 Autofac / Windsor / LightInject 时如何集成 AOP。
- [配置注入](./configuration-injection.md) — 从 `IConfiguration` 注入字段值。
- [总体架构](../architecture/overview.md) — 容器与代理生成的协作方式。
