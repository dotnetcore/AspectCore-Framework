# 快速上手

本页用一个最小可运行示例，带你走通 AspectCore 的完整链路：定义拦截器、把它应用到方法上、用 `Microsoft.Extensions.DependencyInjection` 接管容器，然后调用服务看到拦截生效。

## 前置条件

- 一个 .NET 6.0 及以上的项目（控制台程序即可）。
- 已安装 `AspectCore.Extensions.DependencyInjection`：

```bash
dotnet add package AspectCore.Extensions.DependencyInjection
```

安装说明与选包见[安装](./installation.md)。

## 1. 定义一个拦截器

最常见的写法是继承 `AbstractInterceptorAttribute`，它本身是一个 `Attribute`，可以直接标注到接口、类或方法上。在 `Invoke` 中，调用 `next(context)`（或 `context.Invoke(next)`）把控制权交给下一个环节，前后即是你的切面逻辑：

```csharp
using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

public class LoggingInterceptorAttribute : AbstractInterceptorAttribute
{
    public override async Task Invoke(AspectContext context, AspectDelegate next)
    {
        Console.WriteLine($"[Before] {context.ImplementationMethod.Name}");
        try
        {
            await next(context);
        }
        finally
        {
            Console.WriteLine($"[After] {context.ImplementationMethod.Name}");
        }
    }
}
```

## 2. 定义服务并应用拦截器

把拦截器特性标注到接口方法上。AspectCore 会为 `ICustomService` 生成接口代理，在调用 `Call()` 时织入拦截逻辑：

```csharp
using System;
using AspectCore.DynamicProxy;

public interface ICustomService
{
    [LoggingInterceptor]
    void Call();
}

public class CustomService : ICustomService
{
    public void Call() => Console.WriteLine("service calling...");
}
```

## 3. 用 MS.DI 接管容器并运行

注册服务后调用 `ConfigureDynamicProxy()` 启用动态代理。构建 `ServiceProvider` 时用 `BuildDynamicProxyProvider()`（控制台场景最简单），解析到的实例就是已织入的代理：

```csharp
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddTransient<ICustomService, CustomService>();
services.ConfigureDynamicProxy();

var provider = services.BuildDynamicProxyProvider();
provider.GetRequiredService<ICustomService>().Call();
```

运行后输出：

```text
[Before] Call
service calling...
[After] Call
```

## 在 ASP.NET Core / 泛型主机中接管容器

Web 应用通常不手动 `BuildDynamicProxyProvider()`，而是通过 `IServiceProviderFactory` 让主机接管容器构建。在 `Program`/`CreateHostBuilder` 上挂 `DynamicProxyServiceProviderFactory`，其余保持不变：

```csharp
using AspectCore.Extensions.DependencyInjection;

Host.CreateDefaultBuilder(args)
    .UseServiceProviderFactory(new DynamicProxyServiceProviderFactory())
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
```

在 `ConfigureServices` 中照常注册服务，并调用一次 `services.ConfigureDynamicProxy()`：

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<ICustomService, CustomService>();
    services.ConfigureDynamicProxy();
}
```

## 关于代理能拦截什么

AspectCore 的运行时代理只能织入**可重写**的成员：

- **接口代理**：接口上的成员都可拦截（如上例）。
- **类代理**：只拦截非 `sealed` 类的 `virtual` 成员；`sealed` 类、静态成员无法拦截。

细节见 [DynamicProxy 运行时引擎](../architecture/dynamic-proxy.md)。

## 下一步

- [核心概念](./concepts.md) — 弄清切面、拦截器、代理、切面上下文等术语。
- [拦截器基础](../guide/interceptor.md) — 特性拦截器与全局拦截器、`Order`、`AllowMultiple`。
- [拦截器配置](../guide/interceptor-configuration.md) — 三种注册方式与作用范围谓词。
