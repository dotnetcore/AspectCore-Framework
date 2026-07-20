# Castle to AspectCore Migration Checklist

Use this checklist to plan and track your team's migration from Castle DynamicProxy/Windsor
to AspectCore Framework.

## Pre-Migration Assessment

- [ ] **Inventory Castle usage**: List all Castle `IInterceptor` implementations in your codebase
- [ ] **Identify advanced features**: Check for usage of:
  - [ ] `IChangeProxyTarget` (not supported in AspectCore)
  - [ ] Mixins (not supported in AspectCore)
  - [ ] `IInterceptorSelector` (replace with `AspectPredicate`)
  - [ ] Castle-specific lifecycle (PerThread, Pooled, BoundTo)
- [ ] **Audit async interceptors**: Identify Castle interceptors that handle async (need `IAsyncInterceptor`)
- [ ] **Check ref return usage**: Services with `ref`/`ref readonly` returns cannot use Castle
- [ ] **Review test coverage**: Ensure existing interceptor tests pass before migration
- [ ] **Benchmark current performance**: Run `benchmarks/AspectCore.Benchmarks.Competitive/` to establish baseline

## Phase 1: Add AspectCore (Coexistence)

- [ ] Add NuGet packages:
  - [ ] `AspectCore.Extensions.CastleCompat`
  - [ ] `AspectCore.Extensions.DependencyInjection`
  - [ ] `AspectCore.Core`
- [ ] Configure `services.AddDynamicProxy()` in your startup
- [ ] Bridge existing Castle interceptors using `services.AddCastleInterceptor<T>()`
- [ ] Verify all existing tests still pass
- [ ] Verify application starts and functions correctly
- [ ] Monitor for performance regressions in staging

## Phase 2: Migrate Interceptors (one at a time)

For each Castle `IInterceptor`:

- [ ] Create new AspectCore `AbstractInterceptorAttribute` with equivalent logic
- [ ] Map API calls (see migration guide API mapping table):
  - [ ] `invocation.Proceed()` -> `await next(context)`
  - [ ] `invocation.Method` -> `context.ServiceMethod`
  - [ ] `invocation.Arguments` -> `context.Parameters`
  - [ ] `invocation.ReturnValue` -> `context.ReturnValue`
  - [ ] `invocation.InvocationTarget` -> `context.Implementation`
- [ ] Write unit tests for the new interceptor
- [ ] Register new interceptor in `ConfigureDynamicProxy`
- [ ] Remove old Castle interceptor registration
- [ ] Run full test suite
- [ ] Deploy and validate in staging

## Phase 3: Replace Container Registration

- [ ] Replace Windsor `container.Register(Component.For<>()...)` with MSDI `services.Add*()`
- [ ] Map lifestyles:
  - [ ] `LifestyleTransient()` -> `AddTransient<,>()`
  - [ ] `LifestyleSingleton()` -> `AddSingleton<,>()`
  - [ ] `LifestyleScoped()` -> `AddScoped<,>()`
- [ ] Replace `container.Resolve<T>()` with `serviceProvider.GetRequiredService<T>()`
- [ ] Update any `IWindsorInstaller` implementations to use MSDI registration
- [ ] Verify all service resolutions work correctly
- [ ] Run integration tests

## Phase 4: Remove Castle Dependencies

- [ ] Remove `Castle.Core` NuGet package
- [ ] Remove `Castle.Windsor` NuGet package (if used)
- [ ] Remove `AspectCore.Extensions.CastleCompat` (no longer needed)
- [ ] Remove any remaining Castle `using` statements
- [ ] Remove Castle-specific configuration files
- [ ] Run full build to verify no compile errors
- [ ] Run complete test suite
- [ ] Performance test against pre-migration baseline

## Phase 5: Optimize (Optional)

- [ ] Add `[AspectCoreGenerateProxy]` attributes for NativeAOT support
- [ ] Switch to `ProxyEngine.SourceGenerator` for compile-time proxy generation
- [ ] Enable `ProxyEngineOptions.Strict = true` for Source Generator validation
- [ ] Review `[NonAspect]` exclusions for performance-critical paths
- [ ] Consider using `AspectPredicate` patterns instead of per-class attributes
- [ ] Run competitive benchmarks to verify performance improvements
- [ ] Publish NativeAOT build (if applicable)

## Post-Migration Validation

- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Performance meets or exceeds pre-migration baseline
- [ ] No runtime exceptions related to proxy generation
- [ ] Application starts correctly in all environments
- [ ] CI/CD pipeline passes
- [ ] Documentation updated (API docs, architecture diagrams)
- [ ] Team members briefed on new interception patterns

## Rollback Plan

In case of critical issues during migration:

- [ ] Keep Castle packages available (don't remove from source control history)
- [ ] Maintain feature flags to switch between Castle and AspectCore interceptors
- [ ] Document which phase each service/interceptor is at
- [ ] Have a clear rollback procedure for each phase

## Timeline Estimate

| Phase | Typical Duration | Risk Level |
|-------|-----------------|------------|
| Pre-Migration Assessment | 1-2 days | Low |
| Phase 1: Coexistence | 1-2 days | Low |
| Phase 2: Migrate Interceptors | 1-2 days per interceptor | Medium |
| Phase 3: Replace Container | 2-3 days | Medium |
| Phase 4: Remove Castle | 1 day | Low |
| Phase 5: Optimize | 1-3 days | Low |
| Post-Migration Validation | 1-2 days | Low |

**Total estimate**: 1-3 weeks depending on the number of interceptors and complexity
of Windsor registration.

## Resources

- [Feature Comparison](./feature-comparison.md)
- [Migration Guide](./migration-guide.md)
- [Competitive Benchmarks](../../../benchmarks/AspectCore.Benchmarks.Competitive/)
- [AspectCore Documentation](../../getting-started/)
- [Castle DynamicProxy Documentation](https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md)
