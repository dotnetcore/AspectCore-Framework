using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class MiscellaneousCoverageTests
    {
        #region EnableParameterAspectExtensions

        [Fact]
        public void EnableParameterAspect_WithNullConfiguration_ThrowsArgumentNullException()
        {
            IAspectConfiguration configuration = null;
            var ex = Assert.Throws<ArgumentNullException>(() => configuration.EnableParameterAspect());
            Assert.Equal("configuration", ex.ParamName);
        }

        [Fact]
        public void EnableParameterAspect_WithValidConfiguration_AddsInterceptor()
        {
            var configuration = new AspectConfiguration();
            var result = configuration.EnableParameterAspect();
            Assert.Same(configuration, result);
            Assert.True(configuration.Interceptors.Count > 0);
        }

        #endregion

        #region PropertyInjector

        [Fact]
        public void PropertyInjector_Invoke_WithNullImplementation_DoesNotThrow()
        {
            var context = new ServiceContext();
            var resolver = new ServiceResolver(context);
            var factory = resolver.Resolve<IPropertyInjectorFactory>();
            var injector = factory.Create(typeof(object));
            // Should not throw for null - returns early
            injector.Invoke(null);
        }

        #endregion

        #region PropertyResolverSelector

        [Fact]
        public void PropertyResolverSelector_SelectPropertyResolver_WithNullType_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => PropertyResolverSelector.Default.SelectPropertyResolver(null));
            Assert.Equal("implementationType", ex.ParamName);
        }

        [Fact]
        public void PropertyResolverSelector_SelectPropertyResolver_WithNoAttributes_ReturnsEmpty()
        {
            var result = PropertyResolverSelector.Default.SelectPropertyResolver(typeof(TestService));
            Assert.Empty(result);
        }

        [Fact]
        public void PropertyResolverSelector_SelectPropertyResolver_WithAttributes_ReturnsResolvers()
        {
            var result = PropertyResolverSelector.Default.SelectPropertyResolver(typeof(ServiceWithPropertyInjection));
            Assert.NotEmpty(result);
        }

        #endregion

        #region ScopeResolverFactory

        [Fact]
        public void ScopeResolverFactory_CreateScope_WithNullServiceResolver_ThrowsArgumentNullException()
        {
            var factory = new ScopeResolverFactory(null);
            var ex = Assert.Throws<ArgumentNullException>(() => factory.CreateScope());
            Assert.Equal("ServiceResolver", ex.ParamName);
        }

        [Fact]
        public void ScopeResolverFactory_CreateScope_ReturnsNewScope()
        {
            var context = new ServiceContext();
            var resolver = new ServiceResolver(context);
            var factory = new ScopeResolverFactory(resolver);
            var scope = factory.CreateScope();
            Assert.NotNull(scope);
            Assert.NotSame(resolver, scope);
        }

        #endregion

        #region ServiceValidator

        [Fact]
        public void ServiceValidator_TryValidate_WithSealedClass_ReturnsFalse()
        {
            var context = new ServiceContext();
            var validator = new ServiceValidator(new AspectValidatorBuilder(context.Configuration));
            var def = new TypeServiceDefinition(typeof(SealedClass), typeof(SealedClass), Lifetime.Transient);
            var result = validator.TryValidate(def, out Type implType);
            Assert.False(result);
        }

        [Fact]
        public void ServiceValidator_TryValidate_WithNonAspectType_ReturnsFalse()
        {
            var context = new ServiceContext();
            var validator = new ServiceValidator(new AspectValidatorBuilder(context.Configuration));
            var def = new TypeServiceDefinition(typeof(NonAspectType), typeof(NonAspectType), Lifetime.Transient);
            var result = validator.TryValidate(def, out Type implType);
            Assert.False(result);
        }

        #endregion

        #region AttributeAdditionalInterceptorSelector

        [Fact]
        public void AttributeAdditionalInterceptorSelector_SelectFromBase_ReturnsInheritedInterceptors()
        {
            var selector = new AttributeAdditionalInterceptorSelector();
            var interfaceMethod = typeof(ITestService).GetMethod(nameof(ITestService.DoSomething));
            var implMethod = typeof(DerivedTestService).GetMethod(nameof(DerivedTestService.DoSomething));
            var result = selector.Select(interfaceMethod, implMethod).ToList();
            Assert.Single(result);
        }

        #endregion

        #region ParameterInterceptorSelector with property injection

        [Fact]
        public void ParameterInterceptorSelector_Select_WithPropertyInjection_InjectsProperties()
        {
            var cachingProvider = new AspectCachingProvider();
            var propertyInjectorFactory = new PropertyInjectorFactory(new ServiceResolver(new ServiceContext()));
            var selector = new ParameterInterceptorSelector(propertyInjectorFactory, cachingProvider);
            var method = typeof(TestServiceWithParamInterceptor).GetMethod(nameof(TestServiceWithParamInterceptor.MethodWithInterceptor));
            var parameter = method.GetParameters()[0];
            var result = selector.Select(parameter);
            Assert.Single(result);
            Assert.IsType<TestParamInterceptorWithProperty>(result[0]);
        }

        #endregion

        public sealed class SealedClass { }

        [NonAspect]
        public class NonAspectType { }

        public class TestService { }

        public class ServiceWithPropertyInjection
        {
            [FromServiceContext]
            public string InjectedProperty { get; set; }
        }

        public interface ITestService
        {
            void DoSomething();
        }

        public class BaseTestService : ITestService
        {
            [TestInterceptor(Inherited = true)]
            public virtual void DoSomething() { }
        }

        public class DerivedTestService : BaseTestService
        {
            public override void DoSomething() { }
        }

        public class TestInterceptor : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next) => next(context);
        }

        public class TestServiceWithParamInterceptor
        {
            public virtual void MethodWithInterceptor([TestParamInterceptorWithProperty] int value) { }
        }

        public class TestParamInterceptorWithProperty : ParameterInterceptorAttribute
        {
            [FromServiceContext]
            public string InjectedProperty { get; set; }

            public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
                => next(context);
        }
    }
}
