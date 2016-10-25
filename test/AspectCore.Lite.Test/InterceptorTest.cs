using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Test.Fakes;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Test
{
    public class InterceptorTest : IDependencyInjection
    {

        [Fact]
        public void Execute_Test()
        {
            var context = Substitute.For<IAspectContext>();
            context.ReturnParameter.Returns(new ReturnParameterDescriptor(0, MeaninglessService.Parameters[1]));
            IInterceptor interceptor = new TestInterceptor();
            var next = Task.Delay(100);
            var result = interceptor.ExecuteAsync(context, ctx => next);
            Assert.Equal(context.ReturnParameter.Value, 0);
            Assert.Equal(result, next);
        }

        [Fact]
        public void ExecuteAttribute_Test()
        {
            var context = Substitute.For<IAspectContext>();
            context.ReturnParameter.Returns(new ReturnParameterDescriptor(0, MeaninglessService.Parameters[1]));
            TestInterceptorAttribute interceptor = new TestInterceptorAttribute();
            var next = Task.Delay(100);
            var result = interceptor.ExecuteAsync(context, ctx => next);
            Assert.Equal(context.ReturnParameter.Value, 0);
            Assert.Equal(result, next);
        }

        public class TestInterceptor : IInterceptor
        {
            public bool AllowMultiple
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public int Order
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public Task ExecuteAsync(IAspectContext aspectContext, InterceptorDelegate next)
            {
                return next(aspectContext);
            }
        }

        public class TestInterceptorAttribute : InterceptorAttribute
        {
        }
    }
}
