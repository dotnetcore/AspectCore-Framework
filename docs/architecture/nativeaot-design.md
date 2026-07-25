# NativeAOT AOP 设计方案

> 版本：2026-07-20 v4 (final)  
> 状态：已批准  

---

## 一、背景与目标

### 1.1 为什么需要 NativeAOT 支持

- .NET 9/10 将 NativeAOT 作为平台战略方向，运行时 Reflection.Emit 将逐步被淘汰
- AI Native 时代，AOP 框架需要支持嵌入 AI Agent 工具链、Serverless 冷启动场景
- 竞品（Castle DynamicProxy）无 NativeAOT 支持，这是 AspectCore 差异化窗口期

### 1.2 目标

1. Source Generator 引擎路径下，实现**零 Reflection.Emit 依赖**的完整 AOP 能力
2. NativeAOT 发布的应用可编译、可运行，拦截器行为与运行时版本一致
3. 保持 DynamicProxy 运行时引擎的完整能力（非 NativeAOT 场景不受影响）
4. 建立 CI 门禁，防止 NativeAOT 兼容性回归

### 1.3 非目标

- 不改变默认引擎（DynamicProxy 仍为默认）
- 不要求 `AspectCore.Extensions.Reflection` 库在 NativeAOT 下工作（标注即可）
- 不要求运行时 DynamicProxy 引擎支持 NativeAOT（该引擎本质依赖 Emit）

---

## 二、当前阻塞点分析

### 2.1 阻塞点全景

```
SG 生成的代理方法
  → AspectActivatorContext（参数打包）
  → IAspectContextFactory.CreateContext() → RuntimeAspectContext
  → IAspectBuilderFactory → 拦截器管道
  → 管道最内层调用 context.Complete()
  → MethodReflector.Invoke(_implementation, Parameters)  ← DynamicMethod 爆炸点
  → 返回值 async 解包（Expression.Compile）             ← 第二爆炸点
  → context.ReturnValue 赋值
```

### 2.2 具体阻塞代码

| 阻塞点 | 位置 | 机制 | 严重度 |
|--------|------|------|--------|
| 拦截方法调度 | `AspectContext.Runtime.cs:87-88` | `MethodReflector.Invoke()` → DynamicMethod | CRITICAL |
| 异步解包 | `AspectContextRuntimeExtensions.cs:105-127` | `Expression.Compile()` | HIGH |
| Reflection 库 | `AspectCore.Extensions.Reflection/*` 13处 | DynamicMethod | MEDIUM（标注即可） |
| 代理类型生成 | `ProxyTypeCompiler.cs` | AssemblyBuilder | 已被 SG 解决 |

### 2.3 核心矛盾

Source Generator 在编译时已经知道被拦截方法的完整签名，但运行时仍用 `MethodReflector`（通过 `MethodInfo` + DynamicMethod）做通用调度。这是一个**信息在编译时可得，却推迟到运行时处理**的架构缺陷。

---

## 三、设计方案

### 3.1 核心思路：编译时生成强类型调度委托

将 `RuntimeAspectContext.Complete()` 中的 `MethodReflector.Invoke()` 替换为**编译时生成的强类型委托**，在 Source Generator 路径下完全消除 DynamicMethod 依赖。

### 3.2 新增抽象：`IAspectInvokeDelegate`

```csharp
namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 编译时生成的方法调度委托。NativeAOT 路径下替代 MethodReflector。
    /// </summary>
    public interface IAspectInvokeDelegate
    {
        /// <summary>
        /// 调用真实实现方法并返回结果（boxed）。
        /// 对 void 方法返回 null。
        /// 对 ref/out 参数，调用后写回 parameters 数组对应位置。
        /// 对 ref return 方法，返回 unwrapped 值（非 StrongBox），由调用侧负责 ref 传递。
        /// </summary>
        object Invoke(object instance, object[] parameters);
    }
}
```

**设计说明**：

- 签名 `object Invoke(object, object[])` 与 `MethodReflector.Invoke` 一致，对管道透明
- Boxing 行为与 DynamicProxy 路径完全一致，不构成性能回退
- 未来如需消除 boxing，可引入 `IAspectInvokeDelegate<TResult>` 泛型版本，不影响当前设计

### 3.3 `IAspectContextFactory` 接口扩展（DIM 重载）

**前提**：TFM 收窄到 `net6.0;net8.0;net9.0;net10.0`（去掉 netstandard2.0/2.1），所有目标 runtime 支持 Default Interface Methods。

直接在现有接口上用 DIM 新增重载：

```csharp
namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectContextFactory
    {
        // 原有方法不变——DynamicProxy 路径继续使用
        AspectContext CreateContext(AspectActivatorContext activatorContext);

        // 新增 DIM——SG NativeAOT 路径使用
        // 默认实现回退到原方法（第三方实现无需改动即可编译运行）
        AspectContext CreateContext(AspectActivatorContext activatorContext, IAspectInvokeDelegate invokeDelegate)
            => CreateContext(activatorContext);

        void ReleaseContext(AspectContext aspectContext);
    }
}
```

**`AspectContextFactory` 实现**：

```csharp
public sealed class AspectContextFactory : IAspectContextFactory
{
    private readonly IServiceProvider _serviceProvider;

    // 原有方法完全不变——DynamicProxy 路径
    public AspectContext CreateContext(AspectActivatorContext activatorContext)
    {
        return new RuntimeAspectContext(
            _serviceProvider,
            activatorContext.ServiceMethod,
            activatorContext.TargetMethod,
            activatorContext.ProxyMethod,
            activatorContext.PredicateMethod,
            activatorContext.TargetInstance,
            activatorContext.ProxyInstance,
            activatorContext.Parameters ?? emptyParameters);
    }

    // 新增重载——SG NativeAOT 路径
    public AspectContext CreateContext(AspectActivatorContext activatorContext, IAspectInvokeDelegate invokeDelegate)
    {
        return new SourceGeneratedAspectContext(
            _serviceProvider,
            activatorContext.ServiceMethod,
            activatorContext.TargetMethod,
            activatorContext.ProxyMethod,
            activatorContext.PredicateMethod,
            activatorContext.TargetInstance,
            activatorContext.ProxyInstance,
            activatorContext.Parameters ?? emptyParameters,
            invokeDelegate);
    }

    public void ReleaseContext(AspectContext aspectContext)
    {
        (aspectContext as IDisposable)?.Dispose();
    }
}
```

**`ScopeAspectContextFactory` 适配**（持有具体类型 `AspectContextFactory _aspectContextFactory`）：

```csharp
public sealed class ScopeAspectContextFactory : IAspectContextFactory
{
    private readonly IAspectScheduler _aspectScheduler;
    private readonly AspectContextFactory _aspectContextFactory;

    // 原有方法不变
    public AspectContext CreateContext(AspectActivatorContext activatorContext)
    {
        var aspectContext = _aspectContextFactory.CreateContext(activatorContext);
        if (!_aspectScheduler.TryEnter(aspectContext))
            throw new InvalidOperationException("Error occurred in the schedule AspectContext.");
        return aspectContext;
    }

    // 新增重载
    public AspectContext CreateContext(AspectActivatorContext activatorContext, IAspectInvokeDelegate invokeDelegate)
    {
        var aspectContext = _aspectContextFactory.CreateContext(activatorContext, invokeDelegate);
        if (!_aspectScheduler.TryEnter(aspectContext))
            throw new InvalidOperationException("Error occurred in the schedule AspectContext.");
        return aspectContext;
    }

    public void ReleaseContext(AspectContext aspectContext) { /* ... */ }
}
```

### 3.4 新增 AspectContext 子类：`SourceGeneratedAspectContext`

```csharp
namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// NativeAOT 兼容的 AspectContext 实现。
    /// 使用编译时生成的调度委托，不依赖 MethodReflector/DynamicMethod。
    /// 使用 NativeAOT 安全的 AwaitIfAsync 实现，不依赖 Expression.Compile()。
    /// </summary>
    internal sealed class SourceGeneratedAspectContext : AspectContext, IDisposable
    {
        private readonly IAspectInvokeDelegate _invokeDelegate;
        private volatile IDictionary<string, object> _data;
        private readonly IServiceProvider _serviceProvider;
        private readonly MethodInfo _implementationMethod;
        private readonly object _implementation;
        private bool _disposedValue = false;

        // 属性与 RuntimeAspectContext 完全一致（略）

        public SourceGeneratedAspectContext(
            IServiceProvider serviceProvider,
            MethodInfo serviceMethod,
            MethodInfo targetMethod,
            MethodInfo proxyMethod,
            MethodInfo predicateMethod,
            object targetInstance,
            object proxyInstance,
            object[] parameters,
            IAspectInvokeDelegate invokeDelegate)
        {
            _serviceProvider = serviceProvider;
            _implementationMethod = targetMethod;
            _implementation = targetInstance;
            ServiceMethod = serviceMethod;
            ProxyMethod = proxyMethod;
            Proxy = proxyInstance;
            Parameters = parameters;
            PredicateMethod = predicateMethod;
            _invokeDelegate = invokeDelegate;
        }

        /// <summary>
        /// 语义与 RuntimeAspectContext.Complete() 完全一致：
        /// 1. 调用实现方法获取返回值
        /// 2. 等待异步完成（但不提取 Result，保留 Task/ValueTask 对象）
        /// 3. 赋值给 ReturnValue
        /// 
        /// 与 RuntimeAspectContext 的区别仅在实现机制：
        /// - 方法调度：IAspectInvokeDelegate（编译时委托）替代 MethodReflector（DynamicMethod）
        /// - 异步等待：NativeAOT 安全实现替代 Expression.Compile() 路径
        /// </summary>
        public override async Task Complete()
        {
            if (_implementation == null || _implementationMethod == null)
            {
                await Break();
                return;
            }
            var returnValue = _invokeDelegate.Invoke(_implementation, Parameters);
            await AwaitIfAsyncNativeAotSafe(returnValue);
            ReturnValue = returnValue;
        }

        /// <summary>
        /// NativeAOT 安全的异步等待实现。
        /// 不使用 Expression.Compile()，对 ValueTask<T> 使用 MethodInfo.Invoke
        /// 调用 .AsTask()（标准反射，非 Emit，NativeAOT 兼容）。
        /// </summary>
        private static async Task AwaitIfAsyncNativeAotSafe(object returnValue)
        {
            switch (returnValue)
            {
                case null:
                    break;
                case Task task:
                    await task;
                    break;
                case ValueTask valueTask:
                    await valueTask;
                    break;
                default:
                    // ValueTask<T> 不匹配 ValueTask（不同类型），走此分支
                    var type = returnValue.GetType();
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>))
                    {
                        // 使用 MethodInfo.Invoke 调用 .AsTask()
                        // MethodInfo.Invoke 是 NativeAOT 安全的（标准反射，非 Emit）
                        var asTaskMethod = type.GetMethod(nameof(ValueTask<int>.AsTask))!;
                        var task = (Task)asTaskMethod.Invoke(returnValue, null)!;
                        await task;
                    }
                    break;
            }
        }

        // Break()、Invoke()、Dispose() 与 RuntimeAspectContext 完全一致（略）
        // 注：Break() 中调用的 returnType.GetDefaultValue() 来自 AspectCore.Extensions.Reflection，
        // 但该方法内部仅使用 Type.GetTypeCode + Activator.CreateInstance（标准反射），不依赖 Emit，
        // NativeAOT 安全。Phase 2 标注 [RequiresDynamicCode] 时需排除此方法。
    }
}
```

### 3.5 SG 生成的代理中直接调用 DIM 重载

SG 生成的代理类只需持有 `IAspectContextFactory`，直接调用新重载——DIM 保证即使第三方实现未 override 也能编译运行（默认回退到无 delegate 路径）：

```csharp
// SG 生成的代理类字段——与之前完全相同
private readonly IAspectContextFactory _aspectContextFactory;

// 代理方法体中——直接调用两参数重载，无需 cast 或 null 检查
var __context = _aspectContextFactory.CreateContext(__ctx, __InvokeDelegates.DoWork);
```

**回退行为**：当第三方 `IAspectContextFactory` 实现未 override DIM 方法时，默认实现调用 `CreateContext(ctx)`，走 `RuntimeAspectContext` + MethodReflector 路径。在非 NativeAOT 环境下完全可用；在 NativeAOT + Strict 模式下会在 `Complete()` 阶段抛出明确异常。

### 3.7 Source Generator 生成调度委托

对每个被拦截方法，Source Generator 生成一个强类型的 `IAspectInvokeDelegate` 实现：

```csharp
// === 普通同步方法 ===
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class __InvokeDelegate_IService_DoWork : IAspectInvokeDelegate
{
    public object Invoke(object instance, object[] parameters)
    {
        var typed = (ServiceImpl)instance;
        var p0 = (string)parameters[0];
        var p1 = (int)parameters[1];
        var result = typed.DoWork(p0, p1);
        return result;  // boxing if value type
    }
}

// === void 方法 ===
internal sealed class __InvokeDelegate_IService_Process : IAspectInvokeDelegate
{
    public object Invoke(object instance, object[] parameters)
    {
        ((ServiceImpl)instance).Process();
        return null;
    }
}

// === ref/out 参数 ===
internal sealed class __InvokeDelegate_IService_TryParse : IAspectInvokeDelegate
{
    public object Invoke(object instance, object[] parameters)
    {
        var typed = (ServiceImpl)instance;
        var p0 = (string)parameters[0];
        var p1_ref = (int)(parameters[1] ?? default(int));
        var result = typed.TryParse(p0, out p1_ref);
        parameters[1] = p1_ref;  // 写回 ref/out 到管道共享的 parameters 数组
        return result;
    }
}

// === async Task<T> / ValueTask<T> 方法 ===
internal sealed class __InvokeDelegate_IService_GetDataAsync : IAspectInvokeDelegate
{
    public object Invoke(object instance, object[] parameters)
    {
        var typed = (ServiceImpl)instance;
        var p0 = (int)parameters[0];
        // 返回 Task<T>/ValueTask<T> 对象本身
        // Complete() → AwaitIfAsyncNativeAotSafe 等待完成
        // ReturnValue 保持原对象，上层 inline activation 做 switch 提取
        return typed.GetDataAsync(p0);
    }
}

// === ref return 方法 ===
internal sealed class __InvokeDelegate_IService_GetRef : IAspectInvokeDelegate
{
    public object Invoke(object instance, object[] parameters)
    {
        // 返回 unwrapped T（boxed），代理侧用 StrongBox 承接
        return ((ServiceImpl)instance).GetValue();
    }
}
```

**委托实例作为静态单例**：

```csharp
private static class __InvokeDelegates
{
    public static readonly IAspectInvokeDelegate DoWork = new __InvokeDelegate_IService_DoWork();
    public static readonly IAspectInvokeDelegate GetDataAsync = new __InvokeDelegate_IService_GetDataAsync();
    // ...
}
```

### 3.8 SG 生成的代理方法体——统一 inline activation

**设计决策**：将 sync 和 async 方法统一为 inline activation，直接调用 `IAspectContextFactory.CreateContext(ctx, delegate)` DIM 重载。不再经过 `AspectActivator`，避免修改 DynamicProxy 路径代码。

**sync 方法**：

```csharp
public string DoWork(string name, int count)
{
    if (!ShouldIntercept(__Meta.Service_DoWork, __Meta.Impl_DoWork))
        return base.DoWork(name, count);

    var __args = new object[] { name, count };
    var __ctx = new AspectActivatorContext(
        __Meta.Service_DoWork, __Meta.Impl_DoWork, __Meta.Proxy_DoWork,
        __Meta.Service_DoWork, _implementation, this, __args);

    var __context = _aspectContextFactory.CreateContext(__ctx, __InvokeDelegates.DoWork);
    try
    {
        var __builder = _aspectBuilderFactory.GetBuilder(
            __Meta.Service_DoWork, __Meta.Impl_DoWork, __Meta.Service_DoWork);
        var __pipeline = __builder.Build();
        var __task = __pipeline(__context);
        if (__task.IsFaulted)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo
                .Capture(__task.Exception!.InnerException!).Throw();
        if (!__task.IsCompleted)
            NoSyncContextScope.Run(__task);
        return (string)__context.ReturnValue;
    }
    catch (AspectInvocationException) { throw; }
    catch (Exception __ex)
    {
        if (ThrowAspectException)
            throw new AspectInvocationException(__context, __ex);
        throw;
    }
    finally { _aspectContextFactory.ReleaseContext(__context); }
}
```

**async Task\<T\> 方法**：

```csharp
public async Task<string> GetDataAsync(int id)
{
    if (!ShouldIntercept(__Meta.Service_GetDataAsync, __Meta.Impl_GetDataAsync))
        return await base.GetDataAsync(id);

    var __args = new object[] { id };
    var __ctx = new AspectActivatorContext(
        __Meta.Service_GetDataAsync, __Meta.Impl_GetDataAsync, __Meta.Proxy_GetDataAsync,
        __Meta.Service_GetDataAsync, _implementation, this, __args);

    var __context = _aspectContextFactory.CreateContext(__ctx, __InvokeDelegates.GetDataAsync);
    try
    {
        var __builder = _aspectBuilderFactory.GetBuilder(
            __Meta.Service_GetDataAsync, __Meta.Impl_GetDataAsync, __Meta.Service_GetDataAsync);
        var __pipeline = __builder.Build();
        await __pipeline(__context);
        // ReturnValue 是 Task<string>（由 Complete() 等待完成后保留原对象）
        // 或者是拦截器直接设置的 string 值
        switch (__context.ReturnValue)
        {
            case Task<string> taskResult: return await taskResult;
            case string directResult: return directResult;
            case null: return default;
            default:
                throw new AspectInvocationException(__context,
                    new InvalidCastException($"Cannot cast ReturnValue of type '{__context.ReturnValue.GetType()}' to 'Task<string>' or 'string'."));
        }
    }
    catch (AspectInvocationException) { throw; }
    catch (Exception __ex)
    {
        if (ThrowAspectException)
            throw new AspectInvocationException(__context, __ex);
        throw;
    }
    finally { _aspectContextFactory.ReleaseContext(__context); }
}
```

**非泛型 Task / ValueTask 返回方法**：

```csharp
public async Task ProcessAsync(int id)
{
    if (!ShouldIntercept(__Meta.Service_ProcessAsync, __Meta.Impl_ProcessAsync))
    { await base.ProcessAsync(id); return; }

    var __args = new object[] { id };
    var __ctx = new AspectActivatorContext(/* ... */);
    var __context = _aspectContextFactory.CreateContext(__ctx, __InvokeDelegates.ProcessAsync);
    try
    {
        var __builder = _aspectBuilderFactory.GetBuilder(/* ... */);
        await __builder.Build()(__context);
        // 非泛型 Task：ReturnValue 是 Task 对象，已在 Complete() 中 await 完成
        // 不需要提取结果值
    }
    catch (AspectInvocationException) { throw; }
    catch (Exception __ex)
    {
        if (ThrowAspectException) throw new AspectInvocationException(__context, __ex);
        throw;
    }
    finally { _aspectContextFactory.ReleaseContext(__context); }
}
```

**IAsyncEnumerable\<T\> 方法**：

C# 禁止在含 catch 子句的 try 块中 yield（CS1626）。因此 SG 将管道执行和迭代拆分为两层——与现有 `AspectActivator.InvokeAsyncEnumerableCore` 模式一致：

```csharp
// 外层入口：iterator 方法，try/finally（无 catch）包裹 yield
public async IAsyncEnumerable<string> StreamDataAsync(int count,
    [EnumeratorCancellation] CancellationToken ct = default)
{
    if (!ShouldIntercept(__Meta.Service_StreamDataAsync, __Meta.Impl_StreamDataAsync))
    {
        await foreach (var item in base.StreamDataAsync(count, ct).WithCancellation(ct))
            yield return item;
        yield break;
    }

    var __args = new object[] { count, ct };
    var __ctx = new AspectActivatorContext(/* ... */);
    var __context = _aspectContextFactory.CreateContext(__ctx, __InvokeDelegates.StreamDataAsync);
    try
    {
        // 管道执行（含 catch）拆到 helper 方法
        var __enumerable = await __ExecutePipelineForStreamDataAsync(__context);
        if (__enumerable == null) yield break;

        // 迭代结果——yield 在 try/finally（无 catch）中，合法
        var __enumerator = __enumerable.WithCancellation(ct).GetAsyncEnumerator();
        try
        {
            while (true)
            {
                string __item;
                try
                {
                    if (!await __enumerator.MoveNextAsync()) break;
                    __item = __enumerator.Current;
                }
                catch (Exception __ex)
                {
                    if (ThrowAspectException && __ex is not AspectInvocationException)
                        throw new AspectInvocationException(__context, __ex);
                    throw;
                }
                yield return __item;  // yield 在 try 外层（此层无 catch）
            }
        }
        finally
        {
            await __enumerator.DisposeAsync();
        }
    }
    finally { _aspectContextFactory.ReleaseContext(__context); }
}

// 内层 helper：执行管道（含 catch），返回 IAsyncEnumerable<T>
private async Task<IAsyncEnumerable<string>> __ExecutePipelineForStreamDataAsync(
    AspectContext __context)
{
    try
    {
        var __builder = _aspectBuilderFactory.GetBuilder(/* ... */);
        var __task = __builder.Build()(__context);
        if (__task.IsFaulted)
            ExceptionDispatchInfo.Capture(__task.Exception!.InnerException!).Throw();
        if (!__task.IsCompleted)
            await __task;
        return __context.ReturnValue as IAsyncEnumerable<string>;
    }
    catch (AspectInvocationException) { throw; }
    catch (Exception __ex)
    {
        if (ThrowAspectException)
            throw new AspectInvocationException(__context, __ex);
        throw;
    }
}
```

### 3.9 开放式泛型方法处理

对开放式泛型方法（`void Process<T>(T item)`），Source Generator 无法为所有可能的 `T` 预生成委托。

**策略**：

1. **SG 为每个在项目中发现的具体类型实参生成委托**：扫描调用点的已知具体类型。

2. **用户可通过 `[AspectCoreGenericHint]` 注册额外类型**：
   ```csharp
   [assembly: AspectCoreGenericHint(typeof(IService), nameof(IService.Process), typeof(int))]
   [assembly: AspectCoreGenericHint(typeof(IService), nameof(IService.Process), typeof(string))]
   ```

3. **运行时 fallback**：对未预生成的类型参数组合，`IAspectInvokeDelegate` 实现内部使用 `MethodInfo.MakeGenericMethod(typeArgs).Invoke()`（标准反射，NativeAOT 兼容——前提是目标方法在 rd.xml/TrimmerRoots 中保留了元数据）。

4. **NativeAOT + Strict 模式**：SG 在编译时发出诊断 `ACSG0101`：
   ```
   warning ACSG0101: Method 'IService.Process<T>' is an open generic. NativeAOT-safe delegates
   are only generated for known type arguments. Add [AspectCoreGenericHint] for concrete types,
   or runtime fallback may throw in NativeAOT environments.
   ```

5. **运行时行为**：
   - `Strict = false`：尝试 `MethodInfo.Invoke()`（标准反射调用，NativeAOT 兼容但性能低）
   - `Strict = true`：抛出 `InvalidOperationException`，提示用户添加 GenericHint

---

## 四、实施分阶段

### Phase 1：编译时调度委托（消除 MethodReflector 和 Expression.Compile 依赖）

| 步骤 | 改动 | 影响范围 |
|------|------|----------|
| 1.1 | 新增 `IAspectInvokeDelegate` 接口 | `AspectCore.Abstractions` |
| 1.2 | `IAspectContextFactory` 新增 DIM `CreateContext` 重载 | `AspectCore.Abstractions` |
| 1.3 | 新增 `SourceGeneratedAspectContext`（含 NativeAOT 安全 AwaitIfAsync） | `AspectCore.Core` |
| 1.4 | `AspectContextFactory` override DIM 重载 | `AspectCore.Core` |
| 1.5 | `ScopeAspectContextFactory` override DIM 重载 | `AspectCore.Extensions.AspectScope` |
| 1.6 | SG 为每个被拦截方法生成 `IAspectInvokeDelegate` 实现 | `AspectCore.SourceGenerator` |
| 1.7 | SG 将 sync/async/IAsyncEnumerable 方法统一为 inline activation + factory cast | `AspectCore.SourceGenerator` |

**验收**：
- SG 引擎路径下，`MethodReflector` 和 `Expression.Compile()` 均不在调用栈中
- DynamicProxy 路径零变更：`IAspectContextFactory` 接口不变，`AspectActivatorContext` struct 不变，`RuntimeAspectContext` 不变
- 现有第三方 `IAspectContextFactory` 实现无需改动即可编译运行（DIM 默认实现兜底，自动回退到原路径）

### Phase 2：注解 + 门禁 + 验证

| 步骤 | 改动 | 影响范围 |
|------|------|----------|
| 2.1 | 为 `AspectCore.Extensions.Reflection` 全库公开 API 加 `[RequiresDynamicCode]` | 标注 |
| 2.2 | 为 `RuntimeAspectContext` 和 `AspectActivator` 中依赖 Reflection 库的路径加 `[RequiresDynamicCode]` | 标注 |
| 2.3 | SG 增加 `ACSG0101` 诊断（开放式泛型方法 NativeAOT 覆盖警告） | `AspectCore.SourceGenerator` |
| 2.4 | 新增 NativeAOT 示例项目（`<PublishAot>true</PublishAot>`） | `samples/` |
| 2.5 | CI 增加 NativeAOT publish + 运行验证步骤 | `.github/workflows/` |

**验收**：NativeAOT 发布的示例项目可运行，拦截器行为正确。

### Phase 3：性能验证

| 步骤 | 改动 | 影响范围 |
|------|------|----------|
| 3.1 | BenchmarkDotNet 对比：SG 新路径 vs SG 旧路径 vs DynamicProxy 路径 | `benchmarks/` |
| 3.2 | 验证 DynamicProxy 路径零性能回退 | CI 门禁 |
| 3.3 | 验证 SG 路径性能不低于旧 SG 路径 | CI 门禁 |

**验收**：两条路径均无统计显著的性能回退。

---

## 五、兼容性设计

### 5.1 TFM 变更

本次改动将所有包的 TFM 从 `net9.0;net8.0;net7.0;net6.0;netstandard2.1;netstandard2.0` 收窄为 `net10.0;net9.0;net8.0;net6.0`。

**理由**：
- NativeAOT 要求 .NET 7+，DIM 要求 .NET Core 3.0+
- netstandard2.0/2.1 的唯一场景是 .NET Framework 遗留项目——不会使用 SG 引擎和 NativeAOT
- .NET 6 虽已 EOL，但作为 AOP 框架的最低门槛仍有覆盖价值（大量存量项目）
- .NET 7 已 EOL，可省略

### 5.2 DynamicProxy 路径兼容性保证

| 维度 | 保证 |
|------|------|
| `AspectActivatorContext` struct | **不变**——不增加字段、不修改构造函数 |
| `IAspectContextFactory` 接口 | **不变**——不新增方法、不修改签名 |
| `RuntimeAspectContext` | **不变**——代码和行为均保留 |
| `AspectActivator` | **不变**——sync/async/IAsyncEnumerable 路径均保留 |
| `ILEmitVisitor` 生成的代理 | **不变**——调用原 `CreateContext` 方法 |
| `MethodReflector` | **不变**——DynamicProxy 路径继续使用 |
| DI 注册和解析 | **不变**——不影响服务注册 |

### 5.3 第三方兼容性

| 场景 | 行为 |
|------|------|
| 第三方 `IAspectContextFactory` 未 override DIM 方法 | DIM 默认实现调用 `CreateContext(ctx)` → `RuntimeAspectContext`。非 NativeAOT 环境正常工作。 |
| 第三方 `IAspectContextFactory` override 了 DIM 方法 | SG 路径完全走 NativeAOT 安全路径 |
| Windsor / Autofac / LightInject 扩展 | 纯 DynamicProxy 使用不受影响。如需 SG NativeAOT 支持，需 override DIM 方法（一次性适配） |

### 5.4 二进制兼容分析

| 包 | 影响 |
|----|------|
| `AspectCore.Abstractions` | TFM 收窄（breaking for ns2.0 consumers）；新增接口 `IAspectInvokeDelegate`；`IAspectContextFactory` 新增 DIM 方法——**对 net6.0+ 消费者非 breaking** |
| `AspectCore.Core` | 新增类 `SourceGeneratedAspectContext`；`AspectContextFactory` 新增方法重载——**非 breaking** |
| `AspectCore.SourceGenerator` | 生成代码变更——用户重新编译后生效 |
| `AspectCore.Extensions.AspectScope` | `ScopeAspectContextFactory` 新增方法重载——**非 breaking** |

### 5.4 引擎选择矩阵

| 引擎 | NativeAOT 兼容 | 默认 | 备注 |
|------|----------------|------|------|
| DynamicProxy | 否 | 是 | 运行时织入，功能最完整 |
| SourceGenerator | 是（Phase 1-2 完成后） | 否 | 编译时生成，需用户 opt-in |
| Auto | 条件兼容 | 否 | SG 优先回退 DP；NativeAOT 下 DP 回退不可用会抛异常 |

### 5.5 NativeAOT 下的用户使用方式

```csharp
var services = new ServiceCollection();
services.AddDynamicProxy();
services.ConfigureDynamicProxyEngine(options =>
{
    options.Engine = ProxyEngine.SourceGenerator;
    options.Strict = true;
});
```

---

## 六、关键设计决策

### D1：为何引入 `IAspectInvokeDelegate` 而非直接内联调用？

拦截器管道的架构决定了 `Complete()` 是在管道最内层被调用的——此时控制权已经离开了 SG 生成的代理方法体。管道是通用的 `AspectDelegate` 链，无法持有强类型的直接调用。因此需要一个通过 Factory → Context 传递的间接调度机制。

### D2：为何用 DIM 而非独立接口？

TFM 收窄到 net6.0+ 后，所有目标 runtime 支持 Default Interface Methods。DIM 相比独立接口的优势：
- SG 代理直接调用 `_aspectContextFactory.CreateContext(ctx, delegate)`，无需 cast
- 第三方 `IAspectContextFactory` 实现无需改动——DIM 默认实现兜底
- 减少 1 个接口定义，方案更简洁

### D3：为何 `SourceGeneratedAspectContext` 不复用 `AspectContextRuntimeExtensions.AwaitIfAsync`？

`AwaitIfAsync` 扩展方法对 `ValueTask<T>` 使用 `Expression.Compile()` 缓存委托做类型转换。这在 NativeAOT 下不可用。`SourceGeneratedAspectContext` 提供自己的 `AwaitIfAsyncNativeAotSafe` 实现，对 `ValueTask<T>` 使用 `MethodInfo.Invoke` 调用 `.AsTask()`——这是标准反射（非 Emit），NativeAOT 兼容。

性能影响：`MethodInfo.Invoke` 比 `Expression.Compile()` 缓存的委托慢，但这只影响 `ValueTask<T>` 返回方法的首次等待路径，且发生在 I/O 等待之后。实际 benchmark 中可忽略。

### D4：为何不修改管道为泛型？

泛型管道（`AspectDelegate<TResult>`）可避免 boxing，但会引入 breaking change（重写所有拦截器接口）。超出 P0-3 范围。Boxing 与 DynamicProxy 路径持平，不是回退。

### D5：为何将 async 方法统一为 inline activation？

当前 SG 的 async 方法走 `AspectActivator.InvokeTask<T>(ctx)`，内部使用 `_aspectContextFactory.CreateContext(ctx)` → `RuntimeAspectContext`。这条路径不知道 `IAspectInvokeDelegate`。

方案对比：
- 改造 `AspectActivator` 增加 delegate 参数 → 侵入 DynamicProxy 路径代码
- 扩展 `AspectActivatorContext` struct → 已否决（二进制不兼容 + 性能）
- async 改为 inline activation → 与 sync 一致，factory cast 传入 delegate，零侵入

选择 inline activation：DynamicProxy 路径 `AspectActivator` 完全不变，SG sync/async 生成模式统一。

### D6：ref return 方法处理

生成的委托返回 unwrapped value（boxed T），代理侧使用 `StrongBox<T>` 承接 `context.ReturnValue` 并通过 `ref __refBox.Value` 返回。与现有 SG 实现一致。

### D7：`IAsyncEnumerable<T>` 处理策略

也走 inline activation（§3.8）。delegate 的 `Invoke()` 返回 `IAsyncEnumerable<T>` 对象，`Complete()` 中 `AwaitIfAsyncNativeAotSafe` 对其不做任何操作（switch 不匹配任何 case），`ReturnValue` 保持原对象。代理方法体通过 `await foreach` 迭代结果。

---

## 七、风险评估

| 风险 | 等级 | 缓解 |
|------|------|------|
| SG 生成代码膨胀 | 中 | 每个方法：委托类 ~10 行 + inline activation ~25-30 行。50 个被拦截方法约 2000 行生成代码——可接受 |
| 开放式泛型覆盖不全 | 中 | Strict 模式 + `ACSG0101` 编译时诊断 + `[AspectCoreGenericHint]` |
| `MethodInfo.Invoke` 在 `ValueTask<T>` await 路径的性能 | 低 | 仅 `ValueTask<T>` 返回方法首次等待涉及，且在 I/O 等待之后，实际可忽略 |
| `IAsyncEnumerable<T>` inline activation 的复杂度 | 低 | 已采用两层拆分模式（helper 方法执行管道 + iterator 方法做 yield），与 AspectActivator 现有实现一致 |
| 拦截器修改 Parameters 后 delegate 行为 | 低 | delegate 接收的是同一个 `parameters` 数组引用（管道共享），修改可见 |
| Boxing 开销 | 低 | 与 DynamicProxy 路径完全一致，非回退 |
| 第三方 factory 在 NativeAOT 下回退到 RuntimeAspectContext | 低 | DIM 默认回退到无 delegate 路径；文档明确要求 NativeAOT 场景 override DIM 方法；非 NativeAOT 环境回退正常 |

---

## 八、替代方案对比

| 方案 | 描述 | 优势 | 劣势 | 结论 |
|------|------|------|------|------|
| **A: 本方案** | 独立接口 + 编译时委托 + inline activation | 零 breaking change、DP 零影响、全 TFM 兼容 | 仍有 boxing；IAsyncEnumerable yield 限制需处理 | 采用 |
| B: 独立接口 | 引入 `ISourceGeneratedContextFactory` + `as` cast | 兼容 netstandard2.0 | 多一个接口、需要 cast、代码更复杂 | 不再需要（TFM 已收窄） |
| C: 扩展 struct | `AspectActivatorContext` 加字段 | 不需改 activation 模式 | 二进制不兼容、性能回退 | 否决 |
| D: 重写管道为泛型 | 全泛型 `AspectContext<T>` | 消除 boxing | Breaking change | 否决 |
| E: 仅标注 | 加 `[RequiresDynamicCode]` | 零改动 | NativeAOT 下 AOP 不可用 | 否决 |

---

## 九、测试策略

### 9.1 单元测试

- `IAspectInvokeDelegate` 各类型方法的委托实现正确性
- `SourceGeneratedAspectContext.Complete()` 与 `RuntimeAspectContext.Complete()` 行为一致性
- `AwaitIfAsyncNativeAotSafe` 对 Task / ValueTask / ValueTask\<T\> 的正确性
- DIM 默认实现回退验证（第三方 factory 未 override 时走 RuntimeAspectContext）
- 开放式泛型方法的 Strict / non-Strict 行为

### 9.2 E2E 测试

- SG + sync 方法 + 拦截器修改 Parameters → delegate 使用修改后的数组
- SG + async Task\<T\> + 拦截器链 → 返回值正确
- SG + async Task（非泛型）→ 正确完成
- SG + ValueTask\<T\> + 拦截器链 → 返回值正确
- SG + ValueTask（非泛型）→ 正确完成
- SG + IAsyncEnumerable\<T\> → 迭代正确
- SG + ref return → StrongBox 传递正确
- SG + ref/out 参数 → 写回正确
- SG + 多拦截器堆叠 → 管道执行顺序正确
- SG + 自定义 factory（未 override DIM）→ 优雅回退到 RuntimeAspectContext
- DynamicProxy 引擎全回归 → 确认零影响
- `AspectInvocationException` 包装行为一致性

### 9.3 性能 Benchmark

- `Invoke<TResult>` hot path：SG 新路径 vs SG 旧路径 vs DynamicProxy
- `InvokeTask<TResult>` hot path：同上
- `AwaitIfAsyncNativeAotSafe` vs `AwaitIfAsync`：ValueTask\<T\> 路径对比

---

## 十、交付物清单

- [ ] `IAspectInvokeDelegate` 接口定义
- [ ] `IAspectContextFactory` DIM 重载（`AspectCore.Abstractions`）
- [ ] `SourceGeneratedAspectContext` 实现（`AspectCore.Core`）
- [ ] `AspectContextFactory` override DIM 重载（`AspectCore.Core`）
- [ ] `ScopeAspectContextFactory` override DIM 重载（`AspectCore.Extensions.AspectScope`）
- [ ] Source Generator: 委托类生成器
- [ ] Source Generator: sync/async/IAsyncEnumerable 统一 inline activation
- [ ] Source Generator: `ACSG0101` 开放式泛型诊断
- [ ] `[RequiresDynamicCode]` 全量标注
- [ ] NativeAOT 示例项目
- [ ] CI NativeAOT 验证流水线
- [ ] BenchmarkDotNet 性能对比
- [ ] 单元测试 + E2E 测试
- [ ] 文档更新

---

*设计方案 v4 (final) — TFM 收窄到 net6.0+，采用 DIM 简化接口设计，去掉独立 ISourceGeneratedContextFactory。已批准，可进入实施。*
