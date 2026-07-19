# Configuration Injection

`AspectCore.Extensions.Configuration` lets you inject values from `IConfiguration` directly into the fields of a service, using attributes to declare "which configuration item this field comes from," with the container performing the binding when it resolves the service. It is built on top of the built-in container `IServiceContext`.

## Installation and Enabling

Install the package:

```bash
dotnet add package AspectCore.Extensions.Configuration
```

Register an `IConfiguration` instance in the built-in container, then call `AddConfigurationInject()` to enable configuration injection:

```csharp
using AspectCore.DependencyInjection;
using AspectCore.Extensions.Configuration;
using Microsoft.Extensions.Configuration;

var container = new ServiceContext();
container.AddInstance<IConfiguration>(configuration);
container.AddConfigurationInject();          // enable configuration injection
container.AddType<ValueConfigService>();

var service = container.Build().Resolve<ValueConfigService>();
```

`AddConfigurationInject()` is an extension method on `IServiceContext` (namespace `AspectCore.Extensions.Configuration`), with the signature `IServiceContext AddConfigurationInject(this IServiceContext context)`.

## Injecting a Single Value: [ConfigurationValue]

`[ConfigurationValue]` (class name `ConfigurationValueAttribute`) binds the value of a configuration item to a field. Its constructor is `ConfigurationValueAttribute(string key, params string[] sections)`: `key` is the configuration key, and `sections` is the hierarchy it resides in:

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

The example above corresponds to this configuration structure:

```json
{
  "creator": {
    "name": "lemon",
    "age": 24
  }
}
```

## Binding an Entire Configuration Section: [ConfigurationBinding]

`[ConfigurationBinding]` (class name `ConfigurationBindingAttribute`) binds an entire configuration section to a field of a complex type. The constructor is `ConfigurationBindingAttribute(params string[] sections)`, requiring only the path of the configuration section:

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

The container will `Get` the entire `creator` section into a `Config` object.

## Injecting into Fields (Not Properties)

The current implementation performs configuration injection only on **fields**: the binding callback scans the instance's public and non-public fields (`BindingFlags.Instance | Public | NonPublic`) and does not handle properties. That is exactly why the examples above all use `private readonly` fields.

- `[ConfigurationValue]` → obtains the value via `configuration.GetValue(fieldType, key)`.
- `[ConfigurationBinding]` → binds the entire section via `configurationSection.Get(fieldType)`.

If you apply these two attributes to properties, injection will not happen—apply them to fields.

## Comparison of the Two Attributes

| Attribute | Class name | Constructor | Purpose |
|------|------|--------|------|
| `[ConfigurationValue]` | `ConfigurationValueAttribute` | `(string key, params string[] sections)` | Binds a single configuration value |
| `[ConfigurationBinding]` | `ConfigurationBindingAttribute` | `(params string[] sections)` | Binds an entire configuration section to a complex type |

## Next Steps

- [Dependency Injection Integration](./dependency-injection.md) — usage of the built-in container `IServiceContext`.
- [Data Validation](./data-validation.md) — another extension built on top of the built-in container.
