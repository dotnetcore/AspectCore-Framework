# 第三方容器

如果项目已经在用 Autofac、Castle Windsor 或 LightInject，不必换成 AspectCore 内置容器，也能启用动态代理。每种容器都有一个对应的集成包，提供一行式的注册入口，并接受与其他宿主一致的 `Action<IAspectConfiguration>` 来配置拦截器。泛型主机（`IHostBuilder`）另有专门的扩展。

各集成入口共享同一套拦截器配置 API，拦截器的定义与注册方式见[拦截器配置](./interceptor-configuration.md)。

## Autofac

包：`AspectCore.Extensions.Autofac`。在 `ContainerBuilder` 上调用 `RegisterDynamicProxy(...)`：

```csharp
using AspectCore.Configuration;
using AspectCore.Extensions.Autofac;
using Autofac;

var containerBuilder = new ContainerBuilder();
// 注册你的服务 ...

containerBuilder.RegisterDynamicProxy(config =>
{
    config.Interceptors.AddTyped<MethodExecuteLoggerInterceptor>(Predicates.ForService("*Service"));
});

var container = containerBuilder.Build();
```

`RegisterDynamicProxy` 有两个重载：一个接受可选的 `Action<IAspectConfiguration> configure`，另一个额外接受一个预先构建的 `IAspectConfiguration`：

```csharp
// ContainerBuilder RegisterDynamicProxy(this ContainerBuilder, Action<IAspectConfiguration> configure = null)
// ContainerBuilder RegisterDynamicProxy(this ContainerBuilder, IAspectConfiguration configuration, Action<IAspectConfiguration> configure = null)
```

## Castle Windsor

包：`AspectCore.Extensions.Windsor`。在容器上调用 `AddAspectCoreFacility(...)`：

```csharp
using AspectCore.Configuration;
using AspectCore.Extensions.Windsor;
using Castle.Windsor;

var container = new WindsorContainer().AddAspectCoreFacility(config =>
{
    config.Interceptors.AddTyped<CustomInterceptorAttribute>();
});
```

不带参数调用即使用默认配置：

```csharp
var container = new WindsorContainer().AddAspectCoreFacility();
```

`AddAspectCoreFacility` 提供 `IWindsorContainer` 和 `IKernel` 两个重载，都接受可选的 `Action<IAspectConfiguration> configure`；facility 是幂等的，重复添加不会重复注册。

## LightInject

包：`AspectCore.Extensions.LightInject`。在 LightInject 的 `IServiceContainer` 上调用 `RegisterDynamicProxy(...)`：

```csharp
using AspectCore.Configuration;
using AspectCore.Extensions.LightInject;
using LightInject;

var container = new ServiceContainer().RegisterDynamicProxy(config =>
{
    config.Interceptors.AddDelegate(
        next => ctx => next(ctx),
        Predicates.ForNameSpace("Your.Namespace"));
});
```

同样也支持不带配置的调用（`container.RegisterDynamicProxy();`），以及额外接受预建 `IAspectConfiguration` 的重载。

## 泛型主机（Hosting）

包：`AspectCore.Extensions.Hosting`。它面向 `IHostBuilder`，把内置容器 `IServiceContext` 接入主机。

用 `UseServiceContext()` 接管容器，并可传入委托做注册与配置：

```csharp
using AspectCore.DependencyInjection;
using AspectCore.Extensions.Hosting;
using Microsoft.Extensions.Hosting;

hostBuilder.UseServiceContext(container =>
{
    container.AddType<IService, Service>();
    container.Configure(config =>
    {
        config.Interceptors.AddDelegate(async (ctx, next) =>
        {
            await next(ctx);
            ctx.ReturnValue = "proxy";
        });
    });
});
```

也可以用 `UseDynamicProxy()` 搭配 `ConfigureDynamicProxy(...)`，在 `IServiceCollection` 上注册拦截器：

```csharp
using AspectCore.Configuration;
using AspectCore.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

hostBuilder
    .UseDynamicProxy()
    .ConfigureDynamicProxy((services, config) =>
    {
        config.Interceptors.AddTyped<CustomInterceptorAttribute>();
    });
```

`UseServiceContext` 另有 `Action<HostBuilderContext, IServiceContext>` 和无参重载；`ConfigureDynamicProxy` 也有带 `HostBuilderContext` 的重载。

## 入口一览

| 容器 | 包 | 入口方法 | 扩展目标 |
|------|----|----------|----------|
| Autofac | `AspectCore.Extensions.Autofac` | `RegisterDynamicProxy(...)` | `ContainerBuilder` |
| Castle Windsor | `AspectCore.Extensions.Windsor` | `AddAspectCoreFacility(...)` | `IWindsorContainer` / `IKernel` |
| LightInject | `AspectCore.Extensions.LightInject` | `RegisterDynamicProxy(...)` | `IServiceContainer` |
| 泛型主机 | `AspectCore.Extensions.Hosting` | `UseServiceContext()` / `UseDynamicProxy()` | `IHostBuilder` |

## 下一步

- [拦截器配置](./interceptor-configuration.md) — `IAspectConfiguration.Interceptors` 的三种注册方式。
- [依赖注入集成](./dependency-injection.md) — 内置容器与 MS.DI 集成。
- [模块与包结构设计](../architecture/module-design.md) — 各集成包的职责边界。
