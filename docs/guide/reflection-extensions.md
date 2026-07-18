# 反射扩展

`AspectCore.Extensions.Reflection` 提供一套高性能反射调用扩展。它以 `GetReflector()` 为入口，把 `MethodInfo`、`ConstructorInfo`、`PropertyInfo`、`FieldInfo`、`Type` 等成员包装成对应的 reflector，用 IL 编译出访问器并缓存，调用性能比原生反射高出约两个数量级，接近硬编码调用。这个包不依赖 AOP，可独立使用。

## 安装

```bash
dotnet add package AspectCore.Extensions.Reflection
```

## 入口：GetReflector()

`GetReflector()` 是一组扩展方法（静态类 `ReflectorExtensions`，命名空间 `AspectCore.Extensions.Reflection`），按成员类型返回对应的 reflector：

| 扩展目标 | 返回类型 |
|----------|----------|
| `Type` / `TypeInfo` | `TypeReflector` |
| `ConstructorInfo` | `ConstructorReflector` |
| `MethodInfo` | `MethodReflector` |
| `PropertyInfo` | `PropertyReflector` |
| `FieldInfo` | `FieldReflector` |
| `ParameterInfo` | `ParameterReflector` |

`MethodInfo` 与 `PropertyInfo` 另有带 `CallOptions` 的重载。

> 特性反射（`CustomAttributeReflector`）不通过 `GetReflector()` 获取，而是通过 `ICustomAttributeReflectorProvider.CustomAttributeReflectors` 暴露。

## 方法调用：MethodReflector

用法类似 `System.Reflection.MethodInfo`，但调用走 IL 编译的访问器：

```csharp
using AspectCore.Extensions.Reflection;

var method = typeof(MethodFakes).GetMethod("GetString");
var reflector = method.GetReflector();
var result = reflector.Invoke(new MethodFakes(), "lemon");   // -> "lemon"
```

`Invoke` 的签名是 `object Invoke(object instance, params object[] parameters)`；静态方法用 `StaticInvoke(params object[] parameters)`。

## 构造器调用：ConstructorReflector

```csharp
var constructor = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[0]);
var reflector = constructor.GetReflector();
var instance = (ConstructorFakes)reflector.Invoke();         // 无参构造
```

`Invoke` 的签名是 `object Invoke(params object[] args)`。

## 属性访问：PropertyReflector

```csharp
var property = typeof(PropertyFakes).GetTypeInfo().GetProperty("InstanceProperty");
var reflector = property.GetReflector();

var value = reflector.GetValue(fakes);         // 读
reflector.SetValue(fakes, "new value");        // 写
```

`GetValue(object instance)` / `SetValue(object instance, object value)`；静态属性用 `GetStaticValue()` / `SetStaticValue(object value)`。

## 字段访问：FieldReflector

```csharp
var field = typeof(FieldFakes).GetTypeInfo().GetField("InstanceField");
var reflector = field.GetReflector();

var value = reflector.GetValue(fakes);         // 读
reflector.SetValue(fakes, "new value");        // 写
```

签名与 `PropertyReflector` 一致：`GetValue` / `SetValue`，静态字段用 `GetStaticValue()` / `SetStaticValue(object value)`。

## 缓存机制

reflector 的构造有成本（要 IL 编译访问器），因此 `GetReflector()` 内部通过一个静态 `ConcurrentDictionary`（`ReflectorCacheUtils<TMemberInfo, TReflector>`）按成员缓存：同一个 `MemberInfo` 反复调用 `GetReflector()` 只会编译一次访问器，之后复用同一个 reflector 实例。所以在热点路径上无需自己缓存 reflector，直接调用即可。

## 性能量级

官方基准（历史环境，仅示意量级）显示 reflector 相对原生反射有约两个数量级的提升，与硬编码调用同数量级。以方法调用为例：

| Method | Mean |
|--------|------|
| Native_Call | 1.05 ns |
| Reflection_Call | 91.95 ns |
| Reflector_Call | 7.15 ns |

获取特性的优化尤为明显。这些数字来自旧的 BenchmarkDotNet 运行，用于说明量级差异，实际表现随运行环境而变。

## 何时使用

- 需要在运行期频繁做反射调用（如序列化、映射、动态分发）且关注性能时。
- 想要「和原生反射相似的 API」但不想承担原生反射的开销时。
- 不需要 AOP，也可以单独引用这个包。

## 下一步

- [依赖注入集成](./dependency-injection.md) — AspectCore 内部同样依赖高性能反射。
- [模块与包结构设计](../architecture/module-design.md) — 反射扩展在整体架构中的位置。
