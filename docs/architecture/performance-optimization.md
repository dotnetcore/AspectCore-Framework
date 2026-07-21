# 整体性能优化技术方案

> 版本：2026-07-21  
> 状态：待评审  

---

## 一、目标

通过利用 .NET 6-10 各版本的新特性，在不改变公共 API 的前提下，系统性消除 DynamicProxy 和 Source Generator 引擎热路径上的分配与开销。采用条件编译实现多 TFM 分层——低版本保持基础实现，高版本自动获得最优性能。

---

## 二、当前热路径分配分析

### 每次拦截调用的分配（Source Generator 引擎）

| 分配点 | 大小 | 可消除？ | 所需特性 |
|--------|------|---------|---------|
| `object[N]` 参数数组 | 24 + 8N bytes | 是 | 泛型特化 / Span |
| 值类型参数 boxing | 每个 16+ bytes | 是 | 泛型管道 |
| `SourceGeneratedAspectContext` 实例 | ~120 bytes | 是 | 对象池 |
| `ShouldIntercept` 调用（ConcurrentDict 查找） | 0 alloc 但有 CPU 开销 | 是 | 编译时常量 |
| `AspectBuilderFactory.GetKey()` Tuple | 32 bytes | 已消除 | pipeline 缓存 |
| 异步状态机 | ~100-200 bytes | 部分 | PoolingAsyncVTMB |

### DynamicProxy 引擎额外开销

| 分配点 | 来源 | 可优化？ |
|--------|------|---------|
| `reflectorTable.GetOrAdd` lambda | 非 static lambda | 是 |
| `AspectBuilderFactory` Tuple key + lambda | 每次调用 | 是 |
| `CacheAspectValidationHandler.GetOrAdd` 闭包 | 每次调用 | 是 |
| `ServiceResolver.GetOrAdd` 闭包 | Singleton/Scoped 解析 | 是 |

---

## 三、优化方案（按优先级排序）

### P0：SG 引擎 — 消除 ShouldIntercept 运行时检查

**问题**：SG 在编译时已经知道哪些方法需要拦截，但生成的代理仍在每次调用时执行 `ShouldIntercept(serviceMethod, implMethod)` — 涉及 2 次 ConcurrentDictionary 查找和 `IsNonAspect` 反射检查。

**方案**：SG 为已确定需要拦截的方法直接生成拦截路径，不生成 `if (!ShouldIntercept(...))` 分支。对非拦截方法根本不 override。

```csharp
// 优化前：每次调用都检查
public override int Add(int a, int b)
{
    if (!ShouldIntercept(__Meta.Service_Add, __Meta.Impl_Add))
        return base.Add(a, b);  // fast path
    // ... interception ...
}

// 优化后：编译时已决定拦截，无需运行时检查
public override int Add(int a, int b)
{
    // 直接拦截，SG 已知此方法需要拦截
    var __args = ...;
    // ... interception ...
}
```

**影响**：消除每次调用的 ConcurrentDictionary 查找 + IsNonAspect 反射。

**TFM 要求**：无（纯 SG 改动）。

---

### P1：对象池化 SourceGeneratedAspectContext

**问题**：每次拦截调用创建新的 `SourceGeneratedAspectContext` 实例（~120 bytes 堆分配 + GC 压力）。

**方案**：使用 `ObjectPool<SourceGeneratedAspectContext>` 复用实例。

```csharp
// AspectContextFactory.cs
#if NET8_0_OR_GREATER
private readonly ObjectPool<SourceGeneratedAspectContext> _contextPool =
    ObjectPool.Create(new ContextPoolPolicy());
#endif

public AspectContext CreateContext(AspectActivatorContext ctx, IAspectInvokeDelegate del)
{
#if NET8_0_OR_GREATER
    var context = _contextPool.Get();
    context.Reset(ctx, del);  // 重置字段而非新建
    return context;
#else
    return new SourceGeneratedAspectContext(...);
#endif
}

public void ReleaseContext(AspectContext ctx)
{
#if NET8_0_OR_GREATER
    if (ctx is SourceGeneratedAspectContext sgCtx)
    {
        sgCtx.Clear();
        _contextPool.Return(sgCtx);
        return;
    }
#endif
    (ctx as IDisposable)?.Dispose();
}
```

**影响**：消除 ~120 bytes/call 堆分配，显著降低 Gen0 GC。

**TFM 要求**：.NET 8+（`Microsoft.Extensions.ObjectPool`），.NET 6 回退到 new。

---

### P2：消除 DynamicProxy 路径的冗余 lambda 分配

**问题**：4 个热路径 ConcurrentDictionary 调用使用非 static lambda，每次调用分配闭包。

**方案**：

```csharp
// 优化前（RuntimeAspectContext.Complete）
var reflector = reflectorTable.GetOrAdd(
    _implementationMethod,
    method => method.GetReflector(method.IsCallvirt() ? CallOptions.Callvirt : CallOptions.Call));

// 优化后：static lambda + TryGetValue 优先
if (!reflectorTable.TryGetValue(_implementationMethod, out var reflector))
{
    reflector = reflectorTable.GetOrAdd(
        _implementationMethod,
        static method => method.GetReflector(method.IsCallvirt() ? CallOptions.Callvirt : CallOptions.Call));
}
```

同样的模式应用于：
- `AspectBuilderFactory.GetBuilder` 的 lambda
- `CacheAspectValidationHandler.Invoke` 的 lambda
- `ServiceResolver.ResolveDefinition` 的 lambda

**影响**：消除 4 个 lambda 闭包分配/call。

**TFM 要求**：C# 9+（static lambda），所有 TFM 均可用（LangVersion 已是 13.0）。

---

### P3：FrozenDictionary 替换稳定查找表

**问题**：多个 ConcurrentDictionary 在启动后内容不再变化，但每次查找仍有锁竞争开销。

**方案**：启动完成后 freeze：

```csharp
#if NET8_0_OR_GREATER
using System.Collections.Frozen;
#endif

internal class AspectCaching : IAspectCaching
{
#if NET8_0_OR_GREATER
    private FrozenDictionary<object, object>? _frozen;
    private ConcurrentDictionary<object, object> _dict = new();

    public void Freeze()
    {
        _frozen = _dict.ToFrozenDictionary();
    }

    public object GetOrAdd(object key, Func<object, object> factory)
    {
        if (_frozen != null && _frozen.TryGetValue(key, out var val))
            return val;
        return _dict.GetOrAdd(key, factory);
    }
#else
    private readonly ConcurrentDictionary<object, object> _dict = new();
    public object GetOrAdd(object key, Func<object, object> factory) => _dict.GetOrAdd(key, factory);
#endif
}
```

适用于：
- `AspectCaching`（拦截器管道缓存）
- `reflectorTable`（MethodReflector 缓存）
- `CacheAspectValidationHandler`（验证缓存）
- `ServiceTable`（IoC 服务定义表）

**影响**：查找性能提升 30-50%（FrozenDictionary 使用完美哈希）。

**TFM 要求**：.NET 8+，.NET 6 回退到 ConcurrentDictionary。

---

### P4：UnsafeAccessor 替代 MethodReflector（部分场景）

**问题**：`MethodReflector` 使用 `DynamicMethod` + IL emit 构建调用委托，初始化重（微秒级），且不兼容 NativeAOT。

**方案**：对 SG 已知的目标方法，生成 `[UnsafeAccessor]` 静态方法作为 NativeAOT 安全的替代：

```csharp
// SG 生成（.NET 8+）
[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Name")]
private static extern void SetName_Accessor(InitOnlyService target, string value);

// 在 delegate 中使用
public object Invoke(object instance, object[] parameters)
{
    SetName_Accessor((InitOnlyService)instance, (string)parameters[0]);
    return null;
}
```

适用场景：
- init-only property setter（当前用 MethodReflector fallback）
- 私有/internal 成员访问
- 非虚方法调用（替代 CallOptions.Call 的 DynamicMethod）

**影响**：init-only setter 路径从 ~1000ns 降到 ~10ns。NativeAOT 完全安全。

**TFM 要求**：.NET 8+（`[UnsafeAccessor]`），低版本回退到 MethodReflector / MethodInfo.Invoke。

---

### P5：PoolingAsyncValueTaskMethodBuilder

**问题**：每个 async 拦截调用生成一个编译器异步状态机对象（100-200 bytes）。

**方案**：在 SG 生成的异步方法上标注 `[AsyncMethodBuilder]`：

```csharp
// SG 生成
[AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder<>))]
public async ValueTask<string> GetDataAsync(int id)
{
    // ... interception ...
}
```

**限制**：仅适用于返回 `ValueTask<T>` / `ValueTask` 的方法。`Task<T>` 方法不能用此优化。

**影响**：async 路径 GC 压力下降 50-80%。

**TFM 要求**：.NET 6+（`PoolingAsyncValueTaskMethodBuilder` 在 .NET 6 引入）。

---

### P6：IoC 容器优化

**问题**：`ServiceResolver` 的 `GetOrAdd` 每次分配闭包；`LinkedList` 缓存不友好；枚举服务每次分配数组。

**方案**：

```csharp
// 1. TryGetValue before GetOrAdd（所有 TFM）
if (!_resolvedSingletonServices.TryGetValue(definition, out var service))
{
    service = _resolvedSingletonServices.GetOrAdd(definition,
        static (def, resolver) => resolver._serviceCallSiteResolver.Resolve(def)(resolver),
        this);
}

// 2. Replace LinkedList with array（所有 TFM）
private readonly Dictionary<Type, ServiceDefinition[]> _services;

// 3. Pre-compute enumerables（所有 TFM）
private readonly Dictionary<Type, object[]> _enumerableCache;
```

**影响**：IoC 解析路径 -30% 延迟，-60% 分配。

**TFM 要求**：基础优化所有 TFM 可用；`FrozenDictionary` 需 .NET 8+。

---

## 四、TFM 分层矩阵

| 优化 | net6.0 | net8.0 | net9.0 | net10.0 |
|------|:---:|:---:|:---:|:---:|
| P0: SG 消除 ShouldIntercept | ✅ | ✅ | ✅ | ✅ |
| P1: Context 对象池 | ❌ (new) | ✅ ObjectPool | ✅ | ✅ |
| P2: static lambda | ✅ | ✅ | ✅ | ✅ |
| P3: FrozenDictionary | ❌ (ConcurrentDict) | ✅ | ✅ | ✅ |
| P4: UnsafeAccessor | ❌ (Reflector) | ✅ | ✅ | ✅ |
| P5: PoolingAsyncVTMB | ✅ | ✅ | ✅ | ✅ |
| P6: IoC 优化 | ✅ (部分) | ✅ (全部) | ✅ | ✅ |

---

## 五、预期收益

| 引擎/场景 | 当前 | 优化后（预估） | 提升 |
|-----------|------|--------------|------|
| SG sync invoke (pre-resolved) | 205 ns / 448 B | ~120 ns / ~80 B | 40% faster, 80% less alloc |
| SG async Task<T> | 1843 ns / 879 B | ~1200 ns / ~400 B | 35% faster, 55% less alloc |
| DP sync invoke | 221 ns / 360 B | ~180 ns / ~200 B | 20% faster, 45% less alloc |
| IoC Singleton resolve | 28 ns / 32 B | ~15 ns / 0 B | 45% faster, zero-alloc |

---

## 六、实施分阶段

### Phase 1（低风险，全 TFM）
- P0: SG 消除 ShouldIntercept
- P2: static lambda 全量替换
- P6: IoC TryGetValue + LinkedList→Array

### Phase 2（.NET 8+ 条件编译）
- P1: Context 对象池
- P3: FrozenDictionary
- P4: UnsafeAccessor（SG init-only setter）

### Phase 3（SG 生成代码优化）
- P5: PoolingAsyncValueTaskMethodBuilder
- 泛型方法 MakeGenericMethod 缓存

---

## 七、风险评估

| 风险 | 等级 | 缓解 |
|------|------|------|
| 对象池化后的 context 状态泄漏 | 中 | Clear() 方法强制重置所有字段；UT 验证 |
| FrozenDictionary freeze 时机不当 | 低 | 第一次拦截调用后 freeze；或手动 API |
| UnsafeAccessor 访问权限限制 | 低 | 仅用于 SG 已知的具体类型；fallback 到 Reflector |
| PoolingAsyncVTMB 改变异常语义 | 低 | 仅用于 ValueTask 返回方法；Task 方法不变 |
| ShouldIntercept 去掉后动态配置失效 | 中 | 保留运行时 validator 作为可选 fallback |

---

*本方案待评审后分阶段实施。*
