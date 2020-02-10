using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    // https://github.com/dotnetcore/AspectCore-Framework/issues/192
    public class InterceptorAttributeWithArrayMemberTests : DynamicProxyTestBase
    {
        public class PropAttribute : AbstractInterceptorAttribute
        {
            public int[] TimesOfProp { get; set; } = { 1, 100, 10000, 1000000 };
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await next(context);
                context.ReturnValue = (int)context.ReturnValue + TimesOfProp.Sum();
            }
        }

        public class FieldAttribute : AbstractInterceptorAttribute
        {
            public int[] TimesOfField = { 1, 100, 10000, 1000000 };
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await next(context);
                context.ReturnValue = (int)context.ReturnValue + TimesOfField.Sum();
            }
        }

        public class CtorAttribute : AbstractInterceptorAttribute
        {
            private readonly int[] _times;

            public CtorAttribute(int[] times)
            {
                _times = times;
            }

            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await next(context);
                context.ReturnValue = (int)context.ReturnValue + _times.Sum();
            }
        }

        public interface IService
        {
            [Prop(TimesOfProp = new[] { 10, 100 })]
            int ExcuteWithProp(int initTimes);
            
            [Field(TimesOfField = new[] { 10, 100 })]
            int ExcuteWithField(int initTimes);

            [Ctor(new[] { 10, 100 })]
            int ExcuteWithCtor(int initTimes);
        }

        public class Service : IService
        {
            public int ExcuteWithProp(int initTimes) => initTimes;
            public int ExcuteWithField(int initTimes) => initTimes;
            public int ExcuteWithCtor(int initTimes) => initTimes;
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(-100)]
        public void InterceptorAttributeWithArrayMember_Property_Test(int initTimes)
        {
            var service = ProxyGenerator.CreateInterfaceProxy<IService, Service>();
            var times = service.ExcuteWithProp(initTimes);
            Assert.Equal(initTimes + 10 + 100, times);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(-100)]
        public void InterceptorAttributeWithArrayMember_Field_Test(int initTimes)
        {
            var service = ProxyGenerator.CreateInterfaceProxy<IService, Service>();
            var times = service.ExcuteWithField(initTimes);
            Assert.Equal(initTimes + 10 + 100, times);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(-100)]
        public void InterceptorAttributeWithArrayMember_Ctor_Test(int initTimes)
        {
            var service = ProxyGenerator.CreateInterfaceProxy<IService, Service>();
            var times = service.ExcuteWithCtor(initTimes);
            Assert.Equal(initTimes + 10 + 100, times);
        }
    }
}
