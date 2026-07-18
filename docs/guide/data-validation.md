# 数据校验

`AspectCore.Extensions.DataAnnotations` 用拦截器在方法执行前对参数做校验，校验规则直接复用 .NET 标准的 `System.ComponentModel.DataAnnotations` 特性（`[Required]`、`[EmailAddress]`、`[StringLength]` 等）。校验结果通过 `IDataState` 暴露给服务，由业务代码决定如何处理，而不是直接抛异常。

## 两个包的关系

数据校验涉及两个包，是「基础设施 + 实现」的关系：

- `AspectCore.Extensions.DataValidation`：校验的基础设施，提供 `IDataValidator`、`IPropertyValidator`、`IDataState`、`DataValidationInterceptorAttribute` 等抽象与拦截器。它**不提供** `IServiceContext` 的一行式注册入口。
- `AspectCore.Extensions.DataAnnotations`：基于 DataValidation 的具体实现，用 `System.ComponentModel.DataAnnotations` 做校验，并提供用户入口 `AddDataAnnotations(...)`。

使用者一般只需安装并调用 `AspectCore.Extensions.DataAnnotations`；共享的拦截器 `DataValidationInterceptorAttribute` 虽然物理上位于 DataValidation 包，但你通过 `AddDataAnnotations` 启用它。

## 安装与启用

```bash
dotnet add package AspectCore.Extensions.DataAnnotations
```

在内置容器上调用 `AddDataAnnotations(...)`：

```csharp
using AspectCore.DependencyInjection;
using AspectCore.Extensions.DataAnnotations;

var services = new ServiceContext();
services.AddType<IAccountService, AccountService>();
services.AddDataAnnotations();

var resolver = services.Build();
var accountService = resolver.Resolve<IAccountService>();
```

`AddDataAnnotations` 的签名是 `IServiceContext AddDataAnnotations(this IServiceContext services, params AspectPredicate[] predicates)`。它会注册校验器实现，并把 `DataValidationInterceptorAttribute` 加入全局拦截器；传入 `predicates` 可限定校验的作用范围（用法同[条件拦截](./conditional-interception.md)）。

## 用 DataAnnotations 特性定义规则

校验规则写在参数模型上，用标准的 `System.ComponentModel.DataAnnotations` 特性：

```csharp
using System.ComponentModel.DataAnnotations;

public class RegisterInput
{
    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(18, MinimumLength = 6)]
    public string Password { get; set; }
}
```

## 读取校验结果：IDataState

拦截器在方法执行前完成校验，并把结果放进服务的 `IDataState DataState` 属性。业务方法据此判断是否通过：

```csharp
using AspectCore.Extensions.DataValidation;

public class AccountService : IAccountService
{
    public IDataState DataState { get; set; }

    public void Register(RegisterInput input)
    {
        if (DataState.IsValid)
        {
            Console.WriteLine($"register.. name:{input.Name}, email:{input.Email}");
            return;
        }

        foreach (var error in DataState.Errors)
        {
            Console.WriteLine($"error.. key:{error.Key}, message:{error.ErrorMessage}");
        }
    }
}
```

`IDataState` 提供两个成员：

- `bool IsValid` — 参数是否全部通过校验。
- `DataValidationErrorCollection Errors` — 校验失败项的集合，每项带 `Key` 与 `ErrorMessage`。

这种「校验后由业务读取 `DataState`」的模式，把「是否放行」的决定权留给了业务代码，而不是由框架直接中断调用。

## 下一步

- [拦截器配置](./interceptor-configuration.md) — 校验拦截器如何被注册与限定范围。
- [依赖注入集成](./dependency-injection.md) — 内置容器 `IServiceContext` 的用法。
- [常见场景](./common-scenarios.md) — 手写一个参数校验拦截器的例子。
