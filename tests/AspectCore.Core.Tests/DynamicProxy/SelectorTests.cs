using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    #region Test Interceptors

    public class SelectorTestInterceptorAttribute : AbstractInterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            return next(context);
        }
    }

    public class SelectorTestInterceptor : AbstractInterceptor
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            return next(context);
        }
    }

    public class SelectorTestInterceptor2 : AbstractInterceptor
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            return next(context);
        }
    }

    #endregion

    #region Test Attributes

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class NonInterceptorAttribute : Attribute
    {
    }

    #endregion

    #region Test Types for AttributeInterceptorSelector

    [SelectorTestInterceptor]
    public class ClassWithClassInterceptor
    {
        public virtual void Method() { }
    }

    public class ClassWithMethodInterceptor
    {
        [SelectorTestInterceptor]
        public virtual void Method() { }
    }

    [SelectorTestInterceptor]
    public class ClassWithBothInterceptors
    {
        [SelectorTestInterceptor]
        public virtual void Method() { }
    }

    public class ClassWithoutInterceptor
    {
        public virtual void Method() { }
    }

    [NonInterceptor]
    public class ClassWithNonInterceptorAttribute
    {
        [NonInterceptor]
        public virtual void Method() { }
    }

    #endregion

    #region Test Types for AttributeAdditionalInterceptorSelector

    public interface IAdditionalTestService
    {
        void Method();
    }

    public class AdditionalTestService : IAdditionalTestService
    {
        [SelectorTestInterceptor]
        public void Method() { }
    }

    public class AdditionalTestServiceNoInterceptor : IAdditionalTestService
    {
        public void Method() { }
    }

    public interface IInheritedTestService
    {
        void Method();
    }

    public class InheritedBaseImplementation
    {
        [SelectorTestInterceptor(Inherited = true)]
        public virtual void Method() { }
    }

    public class InheritedDerivedImplementation : InheritedBaseImplementation, IInheritedTestService
    {
        public override void Method() { }
    }

    #endregion

    #region Test Types for ConfigureInterceptorSelector

    public class ConfigureTestService
    {
        public virtual void Method() { }
    }

    #endregion

    #region ConfigureInterceptorSelector Tests

    public class ConfigureInterceptorSelectorTests
    {
        private static IAspectConfiguration CreateConfiguration()
        {
            return new AspectConfiguration();
        }

        private static IServiceProvider CreateServiceProvider()
        {
            return new EmptyServiceProvider();
        }

        [Fact]
        public void Constructor_NullAspectConfiguration_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new ConfigureInterceptorSelector(null, CreateServiceProvider()));
            Assert.Equal("aspectConfiguration", ex.ParamName);
        }

        [Fact]
        public void Constructor_NullServiceProvider_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new ConfigureInterceptorSelector(CreateConfiguration(), null));
            Assert.Equal("serviceProvider", ex.ParamName);
        }

        [Fact]
        public void Select_EmptyConfiguration_ReturnsEmpty()
        {
            var configuration = CreateConfiguration();
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());
            var method = typeof(ConfigureTestService).GetMethod(nameof(ConfigureTestService.Method));

            var result = selector.Select(method).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Select_PredicateMatches_ReturnsInterceptor()
        {
            var configuration = CreateConfiguration();
            configuration.Interceptors.AddTyped<SelectorTestInterceptor>(
                Predicates.ForNameSpace("AspectCore.Core.Tests.DynamicProxy"));
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());
            var method = typeof(ConfigureTestService).GetMethod(nameof(ConfigureTestService.Method));

            var result = selector.Select(method).ToList();

            Assert.Single(result);
            Assert.IsType<SelectorTestInterceptor>(result[0]);
        }

        [Fact]
        public void Select_PredicateDoesNotMatch_ReturnsEmpty()
        {
            var configuration = CreateConfiguration();
            configuration.Interceptors.AddTyped<SelectorTestInterceptor>(
                Predicates.ForNameSpace("Some.Other.Namespace"));
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());
            var method = typeof(ConfigureTestService).GetMethod(nameof(ConfigureTestService.Method));

            var result = selector.Select(method).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Select_NoPredicates_MethodNotNonAspect_ReturnsInterceptor()
        {
            var configuration = CreateConfiguration();
            configuration.Interceptors.AddTyped<SelectorTestInterceptor>();
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());
            var method = typeof(ConfigureTestService).GetMethod(nameof(ConfigureTestService.Method));

            var result = selector.Select(method).ToList();

            Assert.Single(result);
            Assert.IsType<SelectorTestInterceptor>(result[0]);
        }

        [Fact]
        public void Select_NoPredicates_MethodIsNonAspect_ReturnsEmpty()
        {
            var configuration = CreateConfiguration();
            configuration.Interceptors.AddTyped<SelectorTestInterceptor>();
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());
            // System namespace is in the default NonAspectPredicates
            var method = typeof(object).GetMethod(nameof(object.ToString));

            var result = selector.Select(method).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Select_MultipleInterceptors_MixedPredicates_ReturnsMatching()
        {
            var configuration = CreateConfiguration();
            configuration.Interceptors.AddTyped<SelectorTestInterceptor>(
                Predicates.ForNameSpace("AspectCore.Core.Tests.DynamicProxy"));
            configuration.Interceptors.AddTyped<SelectorTestInterceptor2>(
                Predicates.ForNameSpace("Some.Other.Namespace"));
            configuration.Interceptors.AddTyped<SelectorTestInterceptor2>();
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());
            var method = typeof(ConfigureTestService).GetMethod(nameof(ConfigureTestService.Method));

            var result = selector.Select(method).ToList();

            // First matches (predicate), second doesn't (predicate), third matches (no predicates and not non-aspect)
            Assert.Equal(2, result.Count);
            Assert.IsType<SelectorTestInterceptor>(result[0]);
            Assert.IsType<SelectorTestInterceptor2>(result[1]);
        }

        [Fact]
        public void Select_MultipleInterceptors_AllMatch_ReturnsAllInOrder()
        {
            var configuration = CreateConfiguration();
            configuration.Interceptors.AddTyped<SelectorTestInterceptor>(
                Predicates.ForNameSpace("*"));
            configuration.Interceptors.AddTyped<SelectorTestInterceptor2>(
                Predicates.ForNameSpace("*"));
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());
            var method = typeof(ConfigureTestService).GetMethod(nameof(ConfigureTestService.Method));

            var result = selector.Select(method).ToList();

            Assert.Equal(2, result.Count);
            Assert.IsType<SelectorTestInterceptor>(result[0]);
            Assert.IsType<SelectorTestInterceptor2>(result[1]);
        }

        [Fact]
        public void Select_NullMethod_ThrowsNullReferenceException()
        {
            var configuration = CreateConfiguration();
            configuration.Interceptors.AddTyped<SelectorTestInterceptor>(
                Predicates.ForNameSpace("*"));
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());

            Assert.Throws<NullReferenceException>(() => selector.Select(null).ToList());
        }

        [Fact]
        public void Select_NullMethod_NoPredicates_ThrowsNullReferenceException()
        {
            var configuration = CreateConfiguration();
            configuration.Interceptors.AddTyped<SelectorTestInterceptor>();
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());

            Assert.Throws<NullReferenceException>(() => selector.Select(null).ToList());
        }

        [Fact]
        public void Select_ForMethodPredicate_MatchesMethodName()
        {
            var configuration = CreateConfiguration();
            configuration.Interceptors.AddTyped<SelectorTestInterceptor>(
                Predicates.ForMethod("Method"));
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());
            var method = typeof(ConfigureTestService).GetMethod(nameof(ConfigureTestService.Method));

            var result = selector.Select(method).ToList();

            Assert.Single(result);
            Assert.IsType<SelectorTestInterceptor>(result[0]);
        }

        [Fact]
        public void Select_ForMethodPredicate_DoesNotMatchMethodName()
        {
            var configuration = CreateConfiguration();
            configuration.Interceptors.AddTyped<SelectorTestInterceptor>(
                Predicates.ForMethod("NonExistentMethod"));
            var selector = new ConfigureInterceptorSelector(configuration, CreateServiceProvider());
            var method = typeof(ConfigureTestService).GetMethod(nameof(ConfigureTestService.Method));

            var result = selector.Select(method).ToList();

            Assert.Empty(result);
        }

        private class EmptyServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }
    }

    #endregion

    #region AttributeInterceptorSelector Tests

    public class AttributeInterceptorSelectorTests
    {
        private readonly AttributeInterceptorSelector _selector = new AttributeInterceptorSelector();

        [Fact]
        public void Select_NoInterceptorAttributes_ReturnsEmpty()
        {
            var method = typeof(ClassWithoutInterceptor).GetMethod(nameof(ClassWithoutInterceptor.Method));

            var result = _selector.Select(method).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Select_ClassLevelInterceptorAttribute_ReturnsInterceptor()
        {
            var method = typeof(ClassWithClassInterceptor).GetMethod(nameof(ClassWithClassInterceptor.Method));

            var result = _selector.Select(method).ToList();

            Assert.Single(result);
            Assert.IsType<SelectorTestInterceptorAttribute>(result[0]);
        }

        [Fact]
        public void Select_MethodLevelInterceptorAttribute_ReturnsInterceptor()
        {
            var method = typeof(ClassWithMethodInterceptor).GetMethod(nameof(ClassWithMethodInterceptor.Method));

            var result = _selector.Select(method).ToList();

            Assert.Single(result);
            Assert.IsType<SelectorTestInterceptorAttribute>(result[0]);
        }

        [Fact]
        public void Select_BothClassAndMethodAttributes_ReturnsBoth()
        {
            var method = typeof(ClassWithBothInterceptors).GetMethod(nameof(ClassWithBothInterceptors.Method));

            var result = _selector.Select(method).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, i => Assert.IsType<SelectorTestInterceptorAttribute>(i));
        }

        [Fact]
        public void Select_NonInterceptorAttributes_AreIgnored()
        {
            var method = typeof(ClassWithNonInterceptorAttribute).GetMethod(nameof(ClassWithNonInterceptorAttribute.Method));

            var result = _selector.Select(method).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Select_NullMethod_ThrowsNullReferenceException()
        {
            Assert.Throws<NullReferenceException>(() => _selector.Select(null).ToList());
        }

        [Fact]
        public void Select_ReturnsInterceptorsInOrder_ClassThenMethod()
        {
            var method = typeof(ClassWithBothInterceptors).GetMethod(nameof(ClassWithBothInterceptors.Method));

            var result = _selector.Select(method).ToList();

            Assert.Equal(2, result.Count);
            // Class-level attributes come first, then method-level
            Assert.All(result, i => Assert.IsAssignableFrom<IInterceptor>(i));
        }

        [Fact]
        public void Select_ReturnedInterceptors_ImplementIInterceptor()
        {
            var method = typeof(ClassWithClassInterceptor).GetMethod(nameof(ClassWithClassInterceptor.Method));

            var result = _selector.Select(method).ToList();

            Assert.All(result, i => Assert.IsAssignableFrom<IInterceptor>(i));
        }
    }

    #endregion

    #region AttributeAdditionalInterceptorSelector Tests

    public class AttributeAdditionalInterceptorSelectorTests
    {
        private readonly AttributeAdditionalInterceptorSelector _selector = new AttributeAdditionalInterceptorSelector();

        [Fact]
        public void Select_SameMethodReference_ReturnsEmpty()
        {
            var method = typeof(AdditionalTestService).GetMethod(nameof(AdditionalTestService.Method));

            var result = _selector.Select(method, method).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Select_BothNull_ReturnsEmpty()
        {
            var result = _selector.Select(null, null).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Select_ImplementationMethodHasInterceptor_ReturnsInterceptor()
        {
            var serviceMethod = typeof(IAdditionalTestService).GetMethod(nameof(IAdditionalTestService.Method));
            var implementationMethod = typeof(AdditionalTestService).GetMethod(nameof(AdditionalTestService.Method));

            var result = _selector.Select(serviceMethod, implementationMethod).ToList();

            Assert.Single(result);
            Assert.IsType<SelectorTestInterceptorAttribute>(result[0]);
        }

        [Fact]
        public void Select_ImplementationMethodHasNoInterceptor_ReturnsEmpty()
        {
            var serviceMethod = typeof(IAdditionalTestService).GetMethod(nameof(IAdditionalTestService.Method));
            var implementationMethod = typeof(AdditionalTestServiceNoInterceptor).GetMethod(nameof(AdditionalTestServiceNoInterceptor.Method));

            var result = _selector.Select(serviceMethod, implementationMethod).ToList();

            Assert.Empty(result);
        }

        [Fact]
        public void Select_InterfaceServiceMethod_IncludesInheritedBaseInterceptors()
        {
            var serviceMethod = typeof(IInheritedTestService).GetMethod(nameof(IInheritedTestService.Method));
            var implementationMethod = typeof(InheritedDerivedImplementation).GetMethod(nameof(InheritedDerivedImplementation.Method));

            var result = _selector.Select(serviceMethod, implementationMethod).ToList();

            // The base method has an inherited interceptor attribute (Inherited = true)
            Assert.NotEmpty(result);
            Assert.All(result, i => Assert.IsType<SelectorTestInterceptorAttribute>(i));
        }

        [Fact]
        public void Select_NullServiceMethod_ThrowsNullReferenceException()
        {
            var implementationMethod = typeof(AdditionalTestService).GetMethod(nameof(AdditionalTestService.Method));

            Assert.Throws<NullReferenceException>(() => _selector.Select(null, implementationMethod).ToList());
        }

        [Fact]
        public void Select_NullImplementationMethod_ThrowsNullReferenceException()
        {
            var serviceMethod = typeof(IAdditionalTestService).GetMethod(nameof(IAdditionalTestService.Method));

            Assert.Throws<NullReferenceException>(() => _selector.Select(serviceMethod, null).ToList());
        }

        [Fact]
        public void Select_ReturnedInterceptors_ImplementIInterceptor()
        {
            var serviceMethod = typeof(IAdditionalTestService).GetMethod(nameof(IAdditionalTestService.Method));
            var implementationMethod = typeof(AdditionalTestService).GetMethod(nameof(AdditionalTestService.Method));

            var result = _selector.Select(serviceMethod, implementationMethod).ToList();

            Assert.All(result, i => Assert.IsAssignableFrom<IInterceptor>(i));
        }
    }

    #endregion
}
