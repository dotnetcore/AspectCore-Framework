using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Microsoft.AspNetCore.Testing;
using System.Linq.Expressions;
using AspectCore.Lite.Test.Fakes;

namespace AspectCore.Lite.Test.Abstractions
{
    public class JoinPointTest:IDependencyInjection
    {
        private readonly IServiceProvider serviceProvider;
        public JoinPointTest()
        {
            serviceProvider = this.BuildServiceProvider();
        }

        [Fact]
        public void ProxyMethodInvoker_Test()
        {
            var joinPoint = serviceProvider.GetService<IJoinPoint>();
            var methodInvoker = Substitute.For<IMethodInvoker>();
            methodInvoker.Invoke().Returns(joinPoint);
            joinPoint.MethodInvoker = methodInvoker;
            Assert.Equal(joinPoint.MethodInvoker, methodInvoker);
        }

        [Fact]
        public void AddInterceptor_ThrowsArgumentNullException()
        {
            var joinPoint = serviceProvider.GetService<IJoinPoint>();
            ExceptionAssert.ThrowsArgumentNull(() => joinPoint.AddInterceptor(null), "interceptorDelegate");
        }

        [Fact]
        public void AddInterceptor_Test()
        {
            var joinPoint = serviceProvider.GetService<IJoinPoint>();

            for (int i = 0; i < 5; i++)
            {
                joinPoint.AddInterceptor(@delegate =>
                {
                    return context => Task.FromResult(i);
                });
            }

            var delegates = Expression.Lambda<Func<IList<Func<InterceptorDelegate, InterceptorDelegate>>>>(Expression.Field(Expression.Constant(joinPoint), "delegates")).Compile()();

            Action<Func<InterceptorDelegate, InterceptorDelegate>>[] actions = new Action<Func<InterceptorDelegate, InterceptorDelegate>>[5];

            for (int i = 0; i < 5; i++)
            {
                actions[i] = async d =>
                 {
                     var interceptorDelegate = d(null)(null);
                     var result = await (Task<int>)interceptorDelegate;
                     Assert.Equal(i, result);
                 };
            }

            Assert.Collection(delegates, actions);
        }

        [Fact]
        public async Task Build_WithInterceptor_Test()
        {
            var joinPoint = serviceProvider.GetService<IJoinPoint>();
            var methodInvoker = Substitute.For<IMethodInvoker>();
            methodInvoker.Invoke().Returns(0);
            joinPoint.MethodInvoker = methodInvoker;
            for (int i = 0; i < 5; i++)
            {
                joinPoint.AddInterceptor(next =>
                {
                    return async ctx =>
                    {
                        await next(ctx);
                        var result = (int)ctx.ReturnParameter.Value;
                        ctx.ReturnParameter.Value = result + 1;
                    };
                });
            }
            var context = Substitute.For<IAspectContext>();
            context.ReturnParameter.Returns(new ReturnParameterDescriptor(0, MeaninglessService.Parameters[1]));
            var interceptorDelegate = joinPoint.Build();
            await interceptorDelegate(context);
            int returnValue = (int)context.ReturnParameter.Value;
            Assert.Equal(returnValue, 5);
        }

        [Fact]
        public async Task Build_NotInterceptor_Test()
        {
            var joinPoint = serviceProvider.GetService<IJoinPoint>();
            var methodInvoker = Substitute.For<IMethodInvoker>();
            methodInvoker.Invoke().Returns(0);
            joinPoint.MethodInvoker = methodInvoker;     
            var context = Substitute.For<IAspectContext>();
            context.ReturnParameter.Returns(new ReturnParameterDescriptor(0, MeaninglessService.Parameters[1]));
            var @delegate = joinPoint.Build();
            await @delegate(context);
            int returnValue = (int)context.ReturnParameter.Value;
            Assert.Equal(returnValue, 0);
        }

        [Fact]
        public void Build_ThrowInvalidOperationException()
        {
            var joinPoint = serviceProvider.GetService<IJoinPoint>();
            ExceptionAssert.Throws<InvalidOperationException>(() => joinPoint.Build(), "Calling proxy method failed.Because instance of ProxyMethodInvoker is null.");
        }

    }
}
