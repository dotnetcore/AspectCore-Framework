# DynamicProxy 运行时引擎

DynamicProxy 是 AspectCore 的运行时代理引擎，基于 `System.Reflection.Emit` 在运行时动态生成代理类型的 IL。本文说明它的两大过程：**代理类型生成**与**运行时拦截**。整体定位见 [总体架构](./overview.md)，与编译时引擎的对比见 [两套引擎对比与选型](./engine-comparison.md)。

代码位于 `src/AspectCore.Core/DynamicProxy/`。

## 1. 代理类型生成

### 1.1 入口与编译器

`IProxyTypeGenerator` 是类型工厂，提供三个方法：`CreateInterfaceProxyType(Type)`（无目标）、`CreateInterfaceProxyType(Type, Type)`（带目标）、`CreateClassProxyType(Type, Type)`。默认实现 `ProxyTypeGenerator`（`ProxyTypeGenerator.cs:11`）在构造时用 `IAspectValidatorBuilder.Build()` 得到校验器，并创建 `ProxyTypeCompiler`；它会先做入口校验（拒绝 `ref struct`，要求接口/类形态）再委托编译器。

`ProxyTypeCompiler`（`ProxyBuilder/ProxyTypeCompiler.cs:14`）持有**单一** `ModuleBuilder`（程序集 `AspectCore.DynamicProxy.Generator`，命名空间 `AspectCore.DynamicGenerated`，`AssemblyBuilderAccess.RunAndCollect`），按类型名加锁缓存生成结果，避免重复生成。三种生成策略：

- 无目标接口代理：两阶段——先构建 stub 实现节点并 emit stub 类型，再构建代理节点并 emit。
- 带目标接口代理：走 `InterfaceProxyBuilder`。
- 类代理：走 `ClassProxyBuilder`。

### 1.2 AST 构建

代理不是直接写 IL，而是先构建一棵**代理类型 AST**（`ProxyTypeNode`，含字段/构造器/方法/属性节点），再由访问器发射 IL。AST 构建器实现 `IProxyTypeBuilder`（`ProxyBuilder/Builders/IProxyAstBuilder.cs:5`）：

- **`ClassProxyBuilder`**（`Builders/ClassProxyAstBuilder.cs:13`）：为类代理注入 `_activatorFactory` 与 `_implementation` 字段，且 `_implementation = this`（类代理包裹自身）；给代理类型打上 `[NonAspect]` + `[Dynamically]`；遍历可见且 virtual 的方法/属性；处理 record 的 `<Clone>$` 拷贝方法与协变返回。
- **`InterfaceProxyBuilder`**（`Builders/InterfaceProxyAstBuilder.cs:11`）：带目标的接口代理，构造器种类为 `InterfaceProxyCtorWithFactoryAndTarget`，`_implementation` 指向接口目标实例。
- **`InterfaceImplBuilder`**（`Builders/InterfaceImplAstBuilder.cs:13`）：无目标接口代理的 stub + proxy，并提供共享的 `BuildProxyMethod`/`ResolveImplementationMethod`。

### 1.3 方法体决策

每个方法生成何种方法体，由 `MethodBodyFactory.DecideBody`（`Builders/MethodBodyFactory.cs:15`）决定：

1. 方法标注 `[NonAspect]` → **委托体**（直接转发，不拦截）。
2. 否则 `validator.Validate(serviceMethod, strict:true) || validator.Validate(implementationMethod, false)` 为真 → **切面激活体**（`AspectActivatorBody`，会走拦截）。
3. 其余 → **委托体**。

方法体节点类型（`ProxyBuilder/Nodes/MethodBodyNode.cs:6`）：

| 节点 | 含义 |
|------|------|
| `DirectDelegationBody` | 直接调用目标方法（目标类型可见时） |
| `ReflectorDelegationBody` | 通过 `MethodReflector` 调用（目标类型不可见时，见 issue #274） |
| `AspectActivatorBody` | 走拦截器管线的方法体 |
| `StubBody` | 无目标接口 stub 的占位实现 |
| `RecordCloneBody` | record 的 `<Clone>$`/`<>Copy` 拷贝方法 |
| `BackingFieldGetBody` / `BackingFieldSetBody` | stub 属性的读写 |

### 1.4 返回类型分发（ReturnKind）

`DetermineReturnKind`（`Builders/MethodBodyFactory.cs:78`）把方法返回类型映射到 `ReturnKind`（`ProxyBuilder/ReturnKind.cs:3`）：

```
void → Void
ref/ref readonly（IsByRef）→ RefSync
Task → Task            Task<T> → TaskOfT
ValueTask → ValueTask  ValueTask<T> → ValueTaskOfT
IAsyncEnumerable<T> → AsyncEnumerable
其余 → Sync
```

### 1.5 IL 发射

`ILEmitVisitor`（`ProxyBuilder/Visitors/ILEmitVisitor.cs`）遍历 AST，通过 `ILGenerator` 发射真实 IL：定义类型/字段/构造器/方法/属性（`VisitProxyType:34`），发射三种构造器（`:113`），并按方法体节点类型发射方法体。

核心是 `VisitAspectActivatorBody`（`:460`）：构造 `AspectActivatorContext` → 加载 `_activatorFactory` → 调 `IAspectActivatorFactory.Create()` → 按 `ReturnKind` 分发（`EmitReturnValue:644`）→ 写回 `byref` 参数 → 返回。对 `ref`/`ref readonly` 返回，会把管线产出的值包进 `StrongBox<T>` 并 `ldflda` 返回其字段地址（因管线是值语义，需堆上存储承载引用，详见 [C# 语言特性适配](./language-features.md)）。

## 2. 运行时拦截

代理方法体被调用时的执行链：

### 2.1 激活器

`AspectActivator`（`AspectActivator.cs:26`）由 `AspectActivatorFactory.Create()` 每次调用创建，是 emit 出的 IL 直接调用的运行时入口。它按返回形态提供四个方法：

- `Invoke<TResult>`（同步/ref）——对未完成的 task 用 `NoSyncContextScope.Run` 规避潜在死锁；取 `(TResult)context.ReturnValue`。
- `InvokeTask<TResult>` / `InvokeValueTask<TResult>`——await 后转对应 `Task<T>`/`ValueTask<T>`。
- `InvokeAsyncEnumerable<TResult>`——流式枚举，逐项异常包装。

异常处理：故障时用 `ExceptionDispatchInfo` 保留原始堆栈；当 `IAspectConfiguration.ThrowAspectException` 为真时包装为 `AspectInvocationException`。`finally` 中 `ReleaseContext`。

### 2.2 上下文

`RuntimeAspectContext`（`AspectContext.Runtime.cs:13`）是 `AspectContext` 的运行时实现：

- `Complete()`（`:80`）：若无目标则 `Break()`；否则取缓存的 `MethodReflector`（按 `IsCallvirt` 选 Callvirt/Call），调用真实目标方法，`await this.AwaitIfAsync(returnValue)`，写入 `ReturnValue`。
- `Break()`（`:93`）：短路管线，为返回类型生成默认值（`ref` 返回会先取元素类型再取默认值）。
- `Invoke(next)` → `next(this)`。

### 2.3 管线组装

`AspectBuilderFactory`（`AspectBuilderFactory.cs:7`）的 `Create(context)` → `GetBuilder(serviceMethod, implementationMethod, predicateMethod)`，结果按方法三元组缓存在 `IAspectCaching` 中。管线种子是 `new AspectBuilder(ctx => ctx.Complete(), null)`，再把收集到的拦截器逐个追加。

`AspectBuilder`（`AspectBuilder.cs:8`）把每个拦截器包成 `next => context => interceptor.Invoke(context, next)`，`Build()` 从尾部（`Complete`）向前折叠成一个 `AspectDelegate` 并缓存。这就是标准中间件链：

```
interceptor₁.Invoke(ctx, next=
  interceptor₂.Invoke(ctx, next=
    …
      ctx.Complete()  // 调用真实目标
  ))
```

### 2.4 拦截器收集

`InterceptorCollector`（`InterceptorCollector.cs:12`）从多个选择器汇总拦截器，再按 `Order` 排序（`HandleSort`）、对非 `AllowMultiple` 去重（`HandleMultiple`），并对需要的拦截器做属性注入，结果缓存。选择器：

- `ConfigureInterceptorSelector`（来自 `IAspectConfiguration.Interceptors`，遵守谓词 + `NonAspectPredicates`）— `ConfigureInterceptorSelector.cs:10`
- `AttributeInterceptorSelector`（类型与方法上的特性拦截器）— `AttributeInterceptorSelector.cs:8`
- `AttributeAdditionalInterceptorSelector`（实现侧 + 继承链）— `AttributeAdditionalInterceptorSelector.cs:9`

### 2.5 异步返回值解包

`AspectContextRuntimeExtensions`（`Extensions/AspectContextRuntimeExtensions.cs:11`）提供 `AwaitIfAsync`/`IsAsync`/`UnwrapAsyncReturnValue`，用表达式编译并缓存 `Task<T>`/`ValueTask<T>` 的结果提取；识别 `AsyncAspectAttribute`。

## 3. 与 IoC 容器的协作

在 DI 场景中，`ServiceTable`（`DependencyInjection/ServiceTable.cs:11`）在注册期通过 `ServiceValidator` 判定服务是否需要代理，需要则调用 `IProxyTypeGenerator` 生成代理类型并包成 `ProxyServiceDefinition`；`ServiceCallSiteResolver` 在解析期通过反射构造代理实例并注入 `IAspectActivatorFactory`。因此使用者从容器解析出来的就是已织入的代理实例。IoC 细节见 [依赖注入集成](../guide/dependency-injection.md)。

## 4. 适用性与限制

- 运行时生成，**无需改动构建流程**，任何被容器管理或经 `ProxyGenerator` 创建的服务都能代理。
- 依赖 `Reflection.Emit`，在完全 AOT / 裁剪场景受限（相关 API 标注了 `[RequiresDynamicCode]`/`[RequiresUnreferencedCode]`）；这类场景应改用 [Source Generator 引擎](./source-generator.md)。
- 只能代理可继承/可重写的成员：非 sealed 类的 virtual 成员、接口成员；`sealed`、`ref struct`、静态成员不支持。
