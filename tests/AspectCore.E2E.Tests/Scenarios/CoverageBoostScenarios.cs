using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests specifically targeting low-coverage classes to push overall
/// coverage above 80%:
/// - TypeArrayComparer (cache lookup with same/different type arg arrays)
/// - CacheAspectValidationHandler (warmup -> freeze path after 100 calls)
/// - ServiceTable (enumerable resolution, generic open-type resolution)
/// - ServiceResolver (singleton/scoped/transient TryGetValue paths)
/// </summary>
[Collection("InterceptorLog")]
public class CoverageBoostScenarios
{
    // ========================================================================
    // TypeArrayComparer — exercise via repeated generic method interception
    // ========================================================================

    /// <summary>
    /// Calls a generic intercepted method repeatedly with the SAME type args.
    /// This exercises the TypeArrayComparer.Equals() cache-hit path (structural
    /// equality for Type[] keys in MakeGenericMethod cache).
    /// </summary>
    [Fact]
    public void GenericMethod_SameTypeArgs_RepeatedCalls_HitsCachePath()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // First call populates the cache; subsequent calls hit TypeArrayComparer.Equals.
        for (int i = 0; i < 20; i++)
        {
            var result = service.Echo(i);
            Assert.Equal(i, result);
        }
    }

    /// <summary>
    /// Calls a generic intercepted method with DIFFERENT type args to exercise
    /// TypeArrayComparer.Equals() returning false (length match but type mismatch)
    /// and TypeArrayComparer.GetHashCode() for new cache entries.
    /// </summary>
    [Fact]
    public void GenericMethod_DifferentTypeArgs_ExercisesCacheMissPath()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // Each different type arg creates a new cache entry (cache miss + add).
        Assert.Equal(42, service.Echo(42));
        Assert.Equal("hello", service.Echo("hello"));
        Assert.Equal(3.14, service.Echo(3.14));
        Assert.Equal(99L, service.Echo(99L));
        Assert.Equal('x', service.Echo('x'));
        Assert.Equal((byte)7, service.Echo((byte)7));

        // Repeat to hit cache after initial population.
        Assert.Equal(100, service.Echo(100));
        Assert.Equal("world", service.Echo("world"));
        Assert.Equal(2.71, service.Echo(2.71));
    }

    /// <summary>
    /// Exercises TypeArrayComparer.Equals with null/length-mismatch scenarios
    /// indirectly through the generic async method path.
    /// </summary>
    [Fact]
    public async Task GenericAsyncMethod_MultipleTypeArgs_ExercisesCache()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // Async generic methods also use the MakeGenericMethod cache.
        for (int i = 0; i < 10; i++)
        {
            var result = await service.EchoAsync($"item-{i}");
            Assert.Equal($"item-{i}", result);
        }

        // Different type arg for async method.
        var intResult = await service.EchoAsync(42);
        Assert.Equal(42, intResult);
    }

    // ========================================================================
    // CacheAspectValidationHandler — warmup -> freeze path
    // ========================================================================

    /// <summary>
    /// Makes 150+ intercepted calls to trigger the CacheAspectValidationHandler
    /// freeze path (after 100 calls it snapshots to FrozenDictionary on net8.0+).
    /// This exercises:
    /// - detectorCache.TryGetValue (cache hit path)
    /// - _callCount increment
    /// - FreezeThreshold check
    /// - _frozen snapshot creation
    /// - frozen.TryGetValue (post-freeze fast path)
    /// </summary>
    [Fact]
    public void RepeatedInterceptedCalls_TriggersCacheFreezePath()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // 160 calls exceeds the FreezeThreshold of 100, triggering the freeze.
        for (int i = 0; i < 160; i++)
        {
            var result = service.Add(i, 1);
            Assert.Equal(i + 1, result);
        }
    }

    /// <summary>
    /// Similar freeze-path test but using multiple different methods to exercise
    /// more cache entries in CacheAspectValidationHandler (each unique
    /// AspectValidationContext is a distinct cache key).
    /// </summary>
    [Fact]
    public void RepeatedCalls_MultipleMethods_ExercisesCacheValidation()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // Interleave calls to different methods to populate multiple cache entries.
        for (int i = 0; i < 60; i++)
        {
            Assert.Equal(i + i, service.Add(i, i));
            Assert.Equal($"a{i}b{i}", service.Concat($"a{i}", $"b{i}"));
            Assert.Equal(i + 1 + 2 + 3, service.MultiParam(i + 1, 2, 3, "label"));
        }
    }

    /// <summary>
    /// Exercises the freeze path using the DynamicProxy engine (non-SG) to
    /// ensure the cache validation handler is hit from both proxy engines.
    /// </summary>
    [Fact]
    public void DynamicProxy_RepeatedCalls_AlsoHitsCacheValidation()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // 160 calls to exceed the freeze threshold.
        for (int i = 0; i < 160; i++)
        {
            Assert.Equal(i + 2, service.Add(i, 2));
        }
    }

    // ========================================================================
    // ServiceTable — enumerable and generic resolution paths
    // ========================================================================

    /// <summary>
    /// Exercises ServiceTable's IEnumerable resolution path (FindEnumerable).
    /// Registers multiple implementations of the same interface and resolves
    /// IEnumerable&lt;T&gt; to get all of them.
    /// </summary>
    [Fact]
    public void ServiceTable_EnumerableResolution_AllRegistrationsReturned()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Transient);
        context.AddType<ICoverageService, CoverageServiceB>(Lifetime.Transient);
        context.AddType<ICoverageService, CoverageServiceC>(Lifetime.Transient);

        var resolver = context.Build();

        var all = resolver.Resolve<IEnumerable<ICoverageService>>();
        Assert.NotNull(all);
        var list = all!.ToList();
        Assert.Equal(3, list.Count);

        // Verify actual instances are present.
        Assert.Contains(list, s => s.GetName() == "A");
        Assert.Contains(list, s => s.GetName() == "B");
        Assert.Contains(list, s => s.GetName() == "C");
    }

    /// <summary>
    /// Exercises ServiceTable's IManyEnumerable resolution path (FindManyEnumerable).
    /// </summary>
    [Fact]
    public void ServiceTable_ManyEnumerableResolution_Works()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Transient);
        context.AddType<ICoverageService, CoverageServiceB>(Lifetime.Transient);

        var resolver = context.Build();

        var many = resolver.ResolveMany<ICoverageService>();
        Assert.NotNull(many);
        var list = many.ToList();
        Assert.Equal(2, list.Count);
    }

    /// <summary>
    /// Exercises ServiceTable's open-generic registration path and subsequent
    /// closed-type resolution (FindGenericService).
    /// </summary>
    [Fact]
    public void ServiceTable_OpenGenericResolution_MultipleClosedTypes()
    {
        var context = new ServiceContext();
        context.AddType(typeof(IGenericCoverageService<>), typeof(GenericCoverageService<>), Lifetime.Transient);

        var resolver = context.Build();

        var intService = resolver.Resolve<IGenericCoverageService<int>>();
        Assert.NotNull(intService);
        Assert.Equal("Int32", intService!.GetTypeName());

        var stringService = resolver.Resolve<IGenericCoverageService<string>>();
        Assert.NotNull(stringService);
        Assert.Equal("String", stringService!.GetTypeName());

        // Resolve again to hit the cached path in FindGenericService.
        var intService2 = resolver.Resolve<IGenericCoverageService<int>>();
        Assert.NotNull(intService2);
        Assert.Equal("Int32", intService2!.GetTypeName());
    }

    /// <summary>
    /// Exercises ServiceTable.Contains path with a generic type that has
    /// IEnumerable resolution.
    /// </summary>
    [Fact]
    public void ServiceTable_Contains_EnumerableGenericType()
    {
        var context = new ServiceContext();
        context.AddType(typeof(IGenericCoverageService<>), typeof(GenericCoverageService<>), Lifetime.Singleton);
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Transient);
        context.AddType<ICoverageService, CoverageServiceB>(Lifetime.Transient);

        var resolver = context.Build();

        // Resolve IEnumerable<ICoverageService> to exercise Contains + FindEnumerable.
        var services = resolver.Resolve<IEnumerable<ICoverageService>>();
        Assert.NotNull(services);
        Assert.Equal(2, services!.Count());

        // Also resolve the open generic.
        var gs = resolver.Resolve<IGenericCoverageService<double>>();
        Assert.NotNull(gs);
        Assert.Equal("Double", gs!.GetTypeName());
    }

    // ========================================================================
    // ServiceResolver — TryGetValue for singleton/scoped/transient
    // ========================================================================

    /// <summary>
    /// Exercises ServiceResolver's singleton resolution TryGetValue path:
    /// first call creates, subsequent calls hit cache.
    /// </summary>
    [Fact]
    public void ServiceResolver_Singleton_CacheHitOnSecondResolve()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Singleton);

        var resolver = context.Build();

        var first = resolver.Resolve<ICoverageService>();
        var second = resolver.Resolve<ICoverageService>();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Same(first, second);
    }

    /// <summary>
    /// Exercises ServiceResolver's scoped resolution path: same instance
    /// within scope, different across scopes.
    /// </summary>
    [Fact]
    public void ServiceResolver_Scoped_SameWithinScope_DifferentAcross()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Scoped);

        var rootResolver = context.Build();

        // Create a child scope via the public extension method.
        using var childResolver = rootResolver.CreateScope();

        var first = childResolver.Resolve(typeof(ICoverageService));
        var second = childResolver.Resolve(typeof(ICoverageService));
        Assert.Same(first, second);

        // Different scope yields a different instance.
        using var childResolver2 = rootResolver.CreateScope();
        var third = childResolver2.Resolve(typeof(ICoverageService));
        Assert.NotSame(first, third);
    }

    /// <summary>
    /// Exercises ServiceResolver's transient resolution path: each resolve
    /// creates a new instance (no caching).
    /// </summary>
    [Fact]
    public void ServiceResolver_Transient_NewInstanceEachTime()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Transient);

        var resolver = context.Build();

        var first = resolver.Resolve<ICoverageService>();
        var second = resolver.Resolve<ICoverageService>();

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
    }

    /// <summary>
    /// Exercises ServiceResolver with null serviceType (returns null).
    /// </summary>
    [Fact]
    public void ServiceResolver_Resolve_NullType_Throws()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Singleton);

        var resolver = context.Build();

        Assert.Throws<ArgumentNullException>(() => resolver.Resolve(null!));
    }

    /// <summary>
    /// Exercises ServiceResolver's Dispose path for singleton and scoped services.
    /// </summary>
    [Fact]
    public void ServiceResolver_Dispose_CleansUpResources()
    {
        var context = new ServiceContext();
        context.AddType<IDisposableCoverageService, DisposableCoverageService>(Lifetime.Singleton);

        var resolver = context.Build();

        var service = resolver.Resolve<IDisposableCoverageService>();
        Assert.NotNull(service);
        Assert.False(service!.IsDisposed);

        resolver.Dispose();
        Assert.True(service.IsDisposed);
    }

    /// <summary>
    /// Exercises ServiceResolver with multiple services of different lifetimes,
    /// testing the interplay between singleton and transient resolution paths.
    /// </summary>
    [Fact]
    public void ServiceResolver_MixedLifetimes_CorrectBehavior()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Singleton);
        context.AddType<IGenericCoverageService<int>, GenericCoverageService<int>>(Lifetime.Transient);

        var resolver = context.Build();

        // Singleton: same instance.
        var s1 = resolver.Resolve<ICoverageService>();
        var s2 = resolver.Resolve<ICoverageService>();
        Assert.Same(s1, s2);

        // Transient: different instances.
        var g1 = resolver.Resolve<IGenericCoverageService<int>>();
        var g2 = resolver.Resolve<IGenericCoverageService<int>>();
        Assert.NotSame(g1, g2);
    }

    /// <summary>
    /// Exercises ServiceResolver resolving a type not registered (returns null).
    /// </summary>
    [Fact]
    public void ServiceResolver_Resolve_UnregisteredType_ReturnsNull()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Singleton);

        var resolver = context.Build();

        var result = resolver.Resolve(typeof(IDisposableCoverageService));
        Assert.Null(result);
    }

    // ========================================================================
    // Combined: generic interception + cache validation + service resolution
    // ========================================================================

    /// <summary>
    /// Combined test: registers a service, resolves it, and makes
    /// 150+ intercepted calls including generic methods with varied type args.
    /// This exercises TypeArrayComparer and CacheAspectValidationHandler simultaneously.
    /// </summary>
    [Fact]
    public void Combined_GenericInterception_CacheFreeze_ServiceResolution()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        var sgService = provider.GetRequiredService<ISgBasicService>();

        // Exercise generic methods with multiple type args (TypeArrayComparer).
        for (int i = 0; i < 30; i++)
        {
            Assert.Equal(i, sgService.Echo(i));
            Assert.Equal($"s{i}", sgService.Echo($"s{i}"));
            Assert.Equal((double)i, sgService.Echo((double)i));
        }

        // Exercise repeated calls past the freeze threshold (CacheAspectValidationHandler).
        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(i + 5, sgService.Add(i, 5));
            Assert.Equal($"a{i}b{i}", sgService.Concat($"a{i}", $"b{i}"));
        }
    }

    /// <summary>
    /// Exercises the DI integration path with enumerable resolution through
    /// the Microsoft DI extension (BuildDynamicProxyProvider), which internally
    /// exercises ServiceTable enumerable paths.
    /// </summary>
    [Fact]
    public void MicrosoftDI_EnumerableResolution_WithInterceptors()
    {
        using var host = new TestHost();
        host.Services.AddTransient<ICoverageService, CoverageServiceA>();
        host.Services.AddTransient<ICoverageService, CoverageServiceB>();
        host.Services.AddTransient<ICoverageService, CoverageServiceC>();

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICoverageService)));
        });

        var services = provider.GetServices<ICoverageService>().ToList();
        Assert.Equal(3, services.Count);
    }

    /// <summary>
    /// Exercises open-generic resolution through the Microsoft DI extension.
    /// </summary>
    [Fact]
    public void MicrosoftDI_OpenGeneric_Resolution_WithInterceptors()
    {
        using var host = new TestHost();
        host.Services.AddTransient(typeof(IGenericCoverageService<>), typeof(GenericCoverageService<>));

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IGenericCoverageService<>)));
        });

        var intSvc = provider.GetService<IGenericCoverageService<int>>();
        var strSvc = provider.GetService<IGenericCoverageService<string>>();

        Assert.NotNull(intSvc);
        Assert.NotNull(strSvc);
        Assert.Equal("Int32", intSvc!.GetTypeName());
        Assert.Equal("String", strSvc!.GetTypeName());
    }

    // ========================================================================
    // ConstructorCallSiteResolver — constructor injection paths
    // ========================================================================

    /// <summary>
    /// Exercises ConstructorCallSiteResolver with a service that has constructor
    /// parameters (resolves dependencies from the container).
    /// </summary>
    [Fact]
    public void ConstructorInjection_SingleConstructor_WithDependencies()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Singleton);
        context.AddType<IServiceWithDependency, ServiceWithDependency>(Lifetime.Transient);

        var resolver = context.Build();

        var service = resolver.Resolve<IServiceWithDependency>();
        Assert.NotNull(service);
        Assert.Equal("A", service!.GetDependencyName());
    }

    /// <summary>
    /// Exercises ConstructorCallSiteResolver with multiple constructors
    /// (sorts by parameter count, picks best match).
    /// </summary>
    [Fact]
    public void ConstructorInjection_MultipleConstructors_BestMatchSelected()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Singleton);
        context.AddType<IMultiCtorService, MultiCtorService>(Lifetime.Transient);

        var resolver = context.Build();

        var service = resolver.Resolve<IMultiCtorService>();
        Assert.NotNull(service);
        // Should use the single-param constructor (has ICoverageService registered).
        Assert.Equal("ctor1:A", service!.GetInfo());
    }

    /// <summary>
    /// Exercises ConstructorCallSiteResolver with a parameter that has a default
    /// value but isn't registered in the container.
    /// </summary>
    [Fact]
    public void ConstructorInjection_DefaultParameterValue_UsedWhenNotRegistered()
    {
        var context = new ServiceContext();
        context.AddType<IDefaultParamService, DefaultParamService>(Lifetime.Transient);

        var resolver = context.Build();

        var service = resolver.Resolve<IDefaultParamService>();
        Assert.NotNull(service);
        Assert.Equal("default-value", service!.GetValue());
    }

    // ========================================================================
    // ServiceTable — keyed services (net8.0+)
    // ========================================================================

    /// <summary>
    /// Exercises ServiceTable's keyed service lookup path by adding
    /// ServiceDefinitions with a non-null ServiceKey.
    /// </summary>
    [Fact]
    public void ServiceTable_KeyedService_ResolvesCorrectImplementation()
    {
        var context = new ServiceContext();
        // Register two implementations with different keys.
        context.Add(new TypeServiceDefinition(typeof(ICoverageService), typeof(CoverageServiceA), Lifetime.Transient, "keyA"));
        context.Add(new TypeServiceDefinition(typeof(ICoverageService), typeof(CoverageServiceB), Lifetime.Transient, "keyB"));

        var resolver = context.Build();

#if NET8_0_OR_GREATER
        // Use the IKeyedServiceProvider interface to resolve by key.
        var keyedProvider = resolver as Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider;
        Assert.NotNull(keyedProvider);

        var serviceA = keyedProvider!.GetKeyedService(typeof(ICoverageService), "keyA") as ICoverageService;
        var serviceB = keyedProvider.GetKeyedService(typeof(ICoverageService), "keyB") as ICoverageService;

        Assert.NotNull(serviceA);
        Assert.NotNull(serviceB);
        Assert.Equal("A", serviceA!.GetName());
        Assert.Equal("B", serviceB!.GetName());
#endif
    }

    /// <summary>
    /// Exercises GetRequiredKeyedService with a valid key.
    /// </summary>
    [Fact]
    public void ServiceTable_GetRequiredKeyedService_Works()
    {
        var context = new ServiceContext();
        context.Add(new TypeServiceDefinition(typeof(ICoverageService), typeof(CoverageServiceA), Lifetime.Singleton, "mykey"));

        var resolver = context.Build();

#if NET8_0_OR_GREATER
        var keyedProvider = resolver as Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider;
        Assert.NotNull(keyedProvider);

        var service = keyedProvider!.GetRequiredKeyedService(typeof(ICoverageService), "mykey") as ICoverageService;
        Assert.NotNull(service);
        Assert.Equal("A", service!.GetName());
#endif
    }

    /// <summary>
    /// Exercises GetRequiredKeyedService with a missing key (throws).
    /// </summary>
    [Fact]
    public void ServiceTable_GetRequiredKeyedService_MissingKey_Throws()
    {
        var context = new ServiceContext();
        context.Add(new TypeServiceDefinition(typeof(ICoverageService), typeof(CoverageServiceA), Lifetime.Singleton, "existing"));

        var resolver = context.Build();

#if NET8_0_OR_GREATER
        var keyedProvider = resolver as Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider;
        Assert.NotNull(keyedProvider);

        Assert.Throws<InvalidOperationException>(() =>
            keyedProvider!.GetRequiredKeyedService(typeof(ICoverageService), "nonexistent"));
#endif
    }

    // ========================================================================
    // ServiceTable — open-generic + IEnumerable resolution (FindEnumerableElements)
    // ========================================================================

    /// <summary>
    /// Exercises ServiceTable's FindEnumerableElements for generic element types:
    /// registers open-generic IGenericCoverageService&lt;&gt; AND a closed-type, then
    /// resolves IEnumerable&lt;IGenericCoverageService&lt;int&gt;&gt;.
    /// This hits the generic branch inside FindEnumerableElements.
    /// </summary>
    [Fact]
    public void ServiceTable_EnumerableOfGenericService_IncludesOpenGenericRegistrations()
    {
        var context = new ServiceContext();
        // Register both open-generic and closed-type implementations.
        context.AddType(typeof(IGenericCoverageService<>), typeof(GenericCoverageService<>), Lifetime.Transient);
        context.AddType<IGenericCoverageService<int>, SpecificIntService>(Lifetime.Transient);

        var resolver = context.Build();

        var services = resolver.Resolve<IEnumerable<IGenericCoverageService<int>>>();
        Assert.NotNull(services);
        var list = services!.ToList();
        // Should resolve both the open-generic (closed to int) and the specific int implementation.
        Assert.True(list.Count >= 2);
    }

    // ========================================================================
    // ServiceTable — delegate-based open-generic registration
    // ========================================================================

    /// <summary>
    /// Exercises ServiceTable's MakGenericService for DelegateServiceDefinition
    /// branch (line 270 in ServiceTable.cs).
    /// </summary>
    [Fact]
    public void ServiceTable_DelegateOpenGeneric_Resolution()
    {
        var context = new ServiceContext();
        // Register as delegate with open-generic type.
        // The delegate must return an instance compatible with the closed generic.
        context.Add(new DelegateServiceDefinition(
            typeof(IGenericCoverageService<>),
            r => new GenericCoverageService<int>(),
            Lifetime.Transient));

        var resolver = context.Build();

        // Resolving the matching closed version exercises MakGenericService's delegate branch.
        var service = resolver.Resolve<IGenericCoverageService<int>>();
        Assert.NotNull(service);
    }

    // ========================================================================
    // ServiceTable — instance-based service with keyed lookup fallthrough
    // ========================================================================

    /// <summary>
    /// Exercises the keyed lookup fallthrough path in TryGetService:
    /// register a non-keyed service, then attempt keyed lookup which falls
    /// through to the generic branch.
    /// </summary>
    [Fact]
    public void ServiceTable_KeyedLookup_FallsThroughToGenericBranch()
    {
        var context = new ServiceContext();
        // Register open-generic with a key.
        context.Add(new TypeServiceDefinition(
            typeof(IGenericCoverageService<>),
            typeof(GenericCoverageService<>),
            Lifetime.Transient,
            "genKey"));

        var resolver = context.Build();

#if NET8_0_OR_GREATER
        var keyedProvider = resolver as Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider;
        Assert.NotNull(keyedProvider);

        // Resolve a closed generic with the key — exercises FindGenericService with serviceKey.
        var service = keyedProvider!.GetKeyedService(typeof(IGenericCoverageService<int>), "genKey") as IGenericCoverageService<int>;
        Assert.NotNull(service);
        Assert.Equal("Int32", service!.GetTypeName());
#endif
    }

    // ========================================================================
    // LifetimeServiceContext — Count, Contains, GetEnumerator paths
    // ========================================================================

    /// <summary>
    /// Exercises LifetimeServiceContext.Count and Contains.
    /// </summary>
    [Fact]
    public void LifetimeServiceContext_Count_And_Contains()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Singleton);
        context.AddType<IGenericCoverageService<int>, GenericCoverageService<int>>(Lifetime.Singleton);

        // Access the Singletons lifetime context.
        Assert.True(context.Singletons.Count > 0);
        Assert.True(context.Singletons.Contains(typeof(ICoverageService)));
        Assert.False(context.Transients.Contains(typeof(ICoverageService)));
    }

    /// <summary>
    /// Exercises LifetimeServiceContext.GetEnumerator path.
    /// </summary>
    [Fact]
    public void LifetimeServiceContext_Enumeration()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Transient);
        context.AddType<ICoverageService, CoverageServiceB>(Lifetime.Transient);

        var count = 0;
        foreach (var def in context.Transients)
        {
            if (def.ServiceType == typeof(ICoverageService))
                count++;
        }
        Assert.Equal(2, count);
    }

    // ========================================================================
    // ServiceInterceptorAttribute — interceptor resolved from DI
    // ========================================================================

    /// <summary>
    /// Exercises the ServiceInterceptorAttribute path which resolves
    /// interceptors from the DI container.
    /// </summary>
    [Fact]
    public void ServiceInterceptor_ResolvedFromDI_Works()
    {
        using var host = new TestHost();
        host.Add<IServiceInterceptedService, ServiceInterceptedService>();
        host.Services.AddSingleton<CountingInterceptor>();

        var service = host.Resolve<IServiceInterceptedService>(config =>
        {
            config.Interceptors.AddServiced<CountingInterceptor>(
                Predicates.Implement(typeof(IServiceInterceptedService)));
        });

        var result = service.GetValue();
        Assert.Equal("intercepted:hello", result);
    }

    // ========================================================================
    // AspectContextRuntimeExtensions — Unwrap for nested proxies
    // ========================================================================

    /// <summary>
    /// Exercises multiple service resolutions with scoped lifetime to
    /// exercise ServiceResolver's scoped resolution caching more thoroughly.
    /// </summary>
    [Fact]
    public void ServiceResolver_ScopedResolution_MultipleTypes()
    {
        var context = new ServiceContext();
        context.AddType<ICoverageService, CoverageServiceA>(Lifetime.Scoped);
        context.AddType<IGenericCoverageService<int>, GenericCoverageService<int>>(Lifetime.Scoped);

        var rootResolver = context.Build();
        using var scope = rootResolver.CreateScope();

        var svc1 = scope.Resolve(typeof(ICoverageService));
        var svc2 = scope.Resolve(typeof(ICoverageService));
        Assert.Same(svc1, svc2);

        var gen1 = scope.Resolve(typeof(IGenericCoverageService<int>));
        var gen2 = scope.Resolve(typeof(IGenericCoverageService<int>));
        Assert.Same(gen1, gen2);
    }

    /// <summary>
    /// Exercises ServiceResolver Dispose with scoped services.
    /// </summary>
    [Fact]
    public void ServiceResolver_ScopedDispose_CleansUpScopedServices()
    {
        var context = new ServiceContext();
        context.AddType<IDisposableCoverageService, DisposableCoverageService>(Lifetime.Scoped);

        var rootResolver = context.Build();
        var scope = rootResolver.CreateScope();

        var service = scope.Resolve(typeof(IDisposableCoverageService)) as DisposableCoverageService;
        Assert.NotNull(service);
        Assert.False(service!.IsDisposed);

        scope.Dispose();
        Assert.True(service.IsDisposed);
    }

    // ========================================================================
    // Additional CacheAspectValidationHandler — post-freeze cache miss path
    // ========================================================================

    /// <summary>
    /// After the freeze threshold is hit, new unique method invocations still
    /// fall back to the ConcurrentDictionary (post-freeze cache miss path).
    /// </summary>
    [Fact]
    public void CacheValidation_PostFreeze_NewMethodStillWorks()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // First: exceed freeze threshold with Add calls.
        for (int i = 0; i < 110; i++)
        {
            service.Add(i, 1);
        }

        // After freeze, call a different method (DoNothing) — tests post-freeze
        // cache-miss fallback path.
        service.DoNothing();
        service.DoNothing();
        Assert.Equal("xy", service.Concat("x", "y"));
    }

    // ========================================================================
    // Helper types
    // ========================================================================

    public interface ICoverageService
    {
        string GetName();
    }

    public class CoverageServiceA : ICoverageService
    {
        public string GetName() => "A";
    }

    public class CoverageServiceB : ICoverageService
    {
        public string GetName() => "B";
    }

    public class CoverageServiceC : ICoverageService
    {
        public string GetName() => "C";
    }

    public interface IGenericCoverageService<T>
    {
        string GetTypeName();
    }

    public class GenericCoverageService<T> : IGenericCoverageService<T>
    {
        public string GetTypeName() => typeof(T).Name;
    }

    public class SpecificIntService : IGenericCoverageService<int>
    {
        public string GetTypeName() => "SpecificInt";
    }

    public interface IDisposableCoverageService
    {
        bool IsDisposed { get; }
    }

    public class DisposableCoverageService : IDisposableCoverageService, IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    // --- Constructor injection test types ---

    public interface IServiceWithDependency
    {
        string GetDependencyName();
    }

    public class ServiceWithDependency : IServiceWithDependency
    {
        private readonly ICoverageService _dep;

        public ServiceWithDependency(ICoverageService dep)
        {
            _dep = dep;
        }

        public string GetDependencyName() => _dep.GetName();
    }

    public interface IMultiCtorService
    {
        string GetInfo();
    }

    public class MultiCtorService : IMultiCtorService
    {
        private readonly string _info;

        public MultiCtorService()
        {
            _info = "ctor0";
        }

        public MultiCtorService(ICoverageService dep)
        {
            _info = $"ctor1:{dep.GetName()}";
        }

        public MultiCtorService(ICoverageService dep, IGenericCoverageService<int> gen)
        {
            _info = $"ctor2:{dep.GetName()}:{gen.GetTypeName()}";
        }

        public string GetInfo() => _info;
    }

    public interface IDefaultParamService
    {
        string GetValue();
    }

    public class DefaultParamService : IDefaultParamService
    {
        private readonly string _value;

        public DefaultParamService(string value = "default-value")
        {
            _value = value;
        }

        public string GetValue() => _value;
    }

    // --- Service interceptor test types ---

    public interface IServiceInterceptedService
    {
        string GetValue();
    }

    public class ServiceInterceptedService : IServiceInterceptedService
    {
        public string GetValue() => "hello";
    }

    public class CountingInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);
            if (context.ReturnValue is string str)
            {
                context.ReturnValue = $"intercepted:{str}";
            }
        }
    }
}
