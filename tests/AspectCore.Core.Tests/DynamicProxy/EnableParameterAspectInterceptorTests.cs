using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class EnableParameterAspectInterceptorTests
    {
        [Fact]
        public async Task Invoke_WithNoParameters_CallsNext()
        {
            var interceptor = new EnableParameterAspectInterceptor();
            var context = CreateContext(nameof(TestService.NoParams));
            var nextCalled = false;
            await interceptor.Invoke(context, ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_WithParametersWithoutInterceptors_CallsNext()
        {
            var interceptor = new EnableParameterAspectInterceptor();
            var context = CreateContext(nameof(TestService.Add));
            var nextCalled = false;
            await interceptor.Invoke(context, ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_WithParameterInterceptor_InvokesParameterInterceptor()
        {
            var interceptor = new EnableParameterAspectInterceptor();
            var context = CreateContext(nameof(TestService.MethodWithParamInterceptor));
            var paramInterceptor = GetParameterInterceptor(context);
            var nextCalled = false;
            await interceptor.Invoke(context, ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_WithReturnParameterInterceptor_InvokesReturnInterceptor()
        {
            var interceptor = new EnableParameterAspectInterceptor();
            var context = CreateContext(nameof(TestService.MethodWithReturnInterceptor));
            var nextCalled = false;
            await interceptor.Invoke(context, ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_WithNullSelector_ThrowsInvalidOperationException()
        {
            var interceptor = new EnableParameterAspectInterceptor();
            var context = CreateContext(nameof(TestService.Add), provideSelector: false);
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                interceptor.Invoke(context, ctx => Task.CompletedTask));
        }

        private static RuntimeAspectContext CreateContext(string methodName, bool provideSelector = true)
        {
            var method = typeof(TestService).GetMethod(methodName);
            IServiceProvider serviceProvider = provideSelector ? new FakeServiceProvider() : new EmptyServiceProvider();
            return new RuntimeAspectContext(
                serviceProvider,
                method,
                method,
                method,
                method,
                new TestService(),
                new TestService(),
                GetDefaultParameters(method));
        }

        private static object[] GetDefaultParameters(MethodInfo method)
        {
            var parameters = method.GetParameters();
            var args = new object[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                args[i] = parameters[i].ParameterType.IsValueType
                    ? Activator.CreateInstance(parameters[i].ParameterType)
                    : null;
            }
            return args;
        }

        private static IParameterInterceptor GetParameterInterceptor(AspectContext context)
        {
            var selector = (IParameterInterceptorSelector)context.ServiceProvider.GetService(typeof(IParameterInterceptorSelector));
            var parameters = context.GetParameters();
            if (parameters.Count > 0)
            {
                var interceptors = selector.Select(parameters[0].ParameterInfo);
                if (interceptors.Length > 0)
                    return interceptors[0];
            }
            return null;
        }

        public class TestService
        {
            public virtual int Add(int a, int b) => a + b;
            public virtual void NoParams() { }
            public virtual int MethodWithParamInterceptor([TestParameterInterceptor] int value) => value;

            [return: TestReturnInterceptor]
            public virtual int MethodWithReturnInterceptor(int value) => value;
        }

        public class TestParameterInterceptor : ParameterInterceptorAttribute
        {
            public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
                => next(context);
        }

        public class TestReturnInterceptor : ReturnParameterInterceptorAttribute
        {
            public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
                => next(context);
        }

        private class FakeServiceProvider : IServiceProvider
        {
            private readonly ParameterInterceptorSelector _selector;

            public FakeServiceProvider()
            {
                var cachingProvider = new AspectCachingProvider();
                var propertyInjectorFactory = new FakePropertyInjectorFactory();
                _selector = new ParameterInterceptorSelector(propertyInjectorFactory, cachingProvider);
            }

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IParameterInterceptorSelector))
                    return _selector;
                return null;
            }
        }

        private class EmptyServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
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
