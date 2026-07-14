using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ParameterInterceptorSelectorTests
    {
        [Fact]
        public void Constructor_WithNullPropertyInjectorFactory_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new ParameterInterceptorSelector(null, new AspectCachingProvider()));
            Assert.Equal("propertyInjectorFactory", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullAspectCachingProvider_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new ParameterInterceptorSelector(new FakePropertyInjectorFactory(), null));
            Assert.Equal("aspectCachingProvider", ex.ParamName);
        }

        [Fact]
        public void Select_WithNullParameter_ThrowsArgumentNullException()
        {
            var selector = new ParameterInterceptorSelector(new FakePropertyInjectorFactory(), new AspectCachingProvider());
            var ex = Assert.Throws<ArgumentNullException>(() => selector.Select(null));
            Assert.Equal("parameter", ex.ParamName);
        }

        [Fact]
        public void Select_WithParameterWithoutInterceptor_ReturnsEmptyArray()
        {
            var selector = new ParameterInterceptorSelector(new FakePropertyInjectorFactory(), new AspectCachingProvider());
            var method = typeof(TestService).GetMethod(nameof(TestService.MethodWithoutInterceptor));
            var parameter = method.GetParameters()[0];
            var result = selector.Select(parameter);
            Assert.Empty(result);
        }

        [Fact]
        public void Select_WithParameterWithInterceptor_ReturnsInterceptors()
        {
            var selector = new ParameterInterceptorSelector(new FakePropertyInjectorFactory(), new AspectCachingProvider());
            var method = typeof(TestService).GetMethod(nameof(TestService.MethodWithInterceptor));
            var parameter = method.GetParameters()[0];
            var result = selector.Select(parameter);
            Assert.Single(result);
            Assert.IsType<TestParameterInterceptor>(result[0]);
        }

        [Fact]
        public void Select_CachesResult()
        {
            var selector = new ParameterInterceptorSelector(new FakePropertyInjectorFactory(), new AspectCachingProvider());
            var method = typeof(TestService).GetMethod(nameof(TestService.MethodWithInterceptor));
            var parameter = method.GetParameters()[0];
            var result1 = selector.Select(parameter);
            var result2 = selector.Select(parameter);
            Assert.Same(result1, result2);
        }

        public class TestService
        {
            public void MethodWithoutInterceptor(string param) { }

            public void MethodWithInterceptor([TestParameterInterceptor] string param) { }
        }

        public class TestParameterInterceptor : ParameterInterceptorAttribute
        {
            public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
            {
                return next(context);
            }
        }

        private class FakePropertyInjectorFactory : IPropertyInjectorFactory
        {
            public IPropertyInjector Create(Type implementationType) => new FakePropertyInjector();
        }

        private class FakePropertyInjector : IPropertyInjector
        {
            public void Invoke(object implementation) { }
        }
    }
}
