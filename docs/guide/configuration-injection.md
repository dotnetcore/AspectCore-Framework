# 配置注入

`AspectCore.Extensions.Configuration` 让你把 `IConfiguration` 中的值直接注入到服务的字段上，用特性声明「这个字段来自哪个配置项」，由容器在解析服务时完成绑定。它构建在内置容器 `IServiceContext` 之上。

## 安装与启用

安装包：

```bash
dotnet add package AspectCore.Extensions.Configuration
```

在内置容器中注册一个 `IConfiguration` 实例，再调用 `AddConfigurationInject()` 启用配置注入：

```csharp
using AspectCore.DependencyInjection;
using AspectCore.Extensions.Configuration;
using Microsoft.Extensions.Configuration;

var container = new ServiceContext();
container.AddInstance<IConfiguration>(configuration);
container.AddConfigurationInject();          // 启用配置注入
container.AddType<ValueConfigService>();

var service = container.Build().Resolve<ValueConfigService>();
```

`AddConfigurationInject()` 是 `IServiceContext` 上的扩展方法（命名空间 `AspectCore.Extensions.Configuration`），签名为 `IServiceContext AddConfigurationInject(this IServiceContext context)`。

## 注入单个值：[ConfigurationValue]

`[ConfigurationValue]`（类名 `ConfigurationValueAttribute`）把一个配置项的值绑定到字段。它的构造器是 `ConfigurationValueAttribute(string key, params string[] sections)`：`key` 是配置键，`sections` 是它所在的层级：

```csharp
using AspectCore.Extensions.Configuration;

public class ValueConfigService
{
    [ConfigurationValue("age", "creator")]
    private readonly int _age = default!;

    [ConfigurationValue("name", "creator")]
    private readonly string _name = default!;

    public override string ToString() => $"{_name}-{_age}";
}
```

上例对应配置结构：

```json
{
  "creator": {
    "name": "lemon",
    "age": 24
  }
}
```

## 绑定整段配置：[ConfigurationBinding]

`[ConfigurationBinding]`（类名 `ConfigurationBindingAttribute`）把一整段配置绑定到一个复杂类型的字段。构造器是 `ConfigurationBindingAttribute(params string[] sections)`，只需给出配置节的路径：

```csharp
using AspectCore.Extensions.Configuration;

public class Config
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class BindConfigService
{
    [ConfigurationBinding("creator")]
    private readonly Config _config = default!;

    public override string ToString() => $"{_config.Name}-{_config.Age}";
}
```

容器会把 `creator` 这一节整体 `Get` 成 `Config` 对象。

## 注入到字段（而非属性）

当前实现只对**字段**做配置注入：绑定回调会扫描实例的公共与非公共字段（`BindingFlags.Instance | Public | NonPublic`），不处理属性。上面的示例都用 `private readonly` 字段，正是这个原因。

- `[ConfigurationValue]` → 通过 `configuration.GetValue(fieldType, key)` 取值。
- `[ConfigurationBinding]` → 通过 `configurationSection.Get(fieldType)` 绑定整段。

如果把这两个特性标注到属性上，注入不会发生——请标注到字段。

## 两个特性对比

| 特性 | 类名 | 构造器 | 作用 |
|------|------|--------|------|
| `[ConfigurationValue]` | `ConfigurationValueAttribute` | `(string key, params string[] sections)` | 绑定单个配置值 |
| `[ConfigurationBinding]` | `ConfigurationBindingAttribute` | `(params string[] sections)` | 把一整段配置绑定到复杂类型 |

## 下一步

- [依赖注入集成](./dependency-injection.md) — 内置容器 `IServiceContext` 的用法。
- [数据校验](./data-validation.md) — 另一个构建在内置容器上的扩展。
