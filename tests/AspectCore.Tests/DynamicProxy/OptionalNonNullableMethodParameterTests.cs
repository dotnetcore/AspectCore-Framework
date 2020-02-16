using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Tests.Issues.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class OptionalNonNullableMethodParameterTests : DynamicProxyTestBase
    {
        public static IEnumerable<object[]> DecimalCases { get; } =
            new decimal[] { 1.23m, 1, 0, -1 }.Select(m => new object[] { m });

        public static IEnumerable<object[]> DateTimeCases { get; } =
            new DateTime[] { default, new DateTime(2000, 1, 1), }.Select(m => new object[] { m });

        public class Interceptor : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return context.Invoke(next);
            }
        }

        public class Service
        {
            [Interceptor] public virtual Bool GetEnumWithDefaultTrue(Bool value = Bool.True) => value;
            [Interceptor] public virtual Bool GetEnumWithDefaultFalse(Bool value = Bool.False) => value;
            [Interceptor] public virtual int GetIntWithDefaultZero(int value = 0) => value;
            [Interceptor] public virtual int GetIntWithDefaultOne(int value = 1) => value;
            [Interceptor] public virtual decimal GetDecimalWithDefaultZero(decimal value = 0) => value;
            [Interceptor] public virtual decimal GetDecimalWithDefaultOne(decimal value = 1) => value;
            [Interceptor] public virtual DateTime GetDateTimeWithDefaultDefault(DateTime value = default) => value;
        }

        public enum Bool
        {
            False,
            True
        }

        [Fact]
        public void OptionalNullableMethodParameter_Enum_UseDefaultValue_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(Bool.True, service.GetEnumWithDefaultTrue());
            Assert.Equal(Bool.False, service.GetEnumWithDefaultFalse());
        }

        [Theory]
        [InlineData(Bool.True)]
        [InlineData(Bool.False)]
        public void OptionalNullableMethodParameter_Enum_UseExplicitValue_Test(Bool input)
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(input, service.GetEnumWithDefaultTrue(input));
            Assert.Equal(input, service.GetEnumWithDefaultFalse(input));
        }

        [Fact]
        public void OptionalNullableMethodParameter_Int_UseDefaultValue_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(1, service.GetIntWithDefaultOne());
            Assert.Equal(0, service.GetIntWithDefaultZero());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        public void OptionalNullableMethodParameter_Int_UseExplicitValue_Test(int input)
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(input, service.GetIntWithDefaultOne(input));
            Assert.Equal(input, service.GetIntWithDefaultZero(input));
        }

        [Fact]
        public void OptionalNullableMethodParameter_Decimal_UseDefaultValue_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(1, service.GetDecimalWithDefaultOne());
            Assert.Equal(0, service.GetDecimalWithDefaultZero());
        }

        [Theory]
        [MemberData(nameof(DecimalCases))]
        public void OptionalNullableMethodParameter_Decimal_UseExplicitValue_Test(decimal input)
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(input, service.GetDecimalWithDefaultOne(input));
            Assert.Equal(input, service.GetDecimalWithDefaultZero(input));
        }

        [Fact]
        public void OptionalNullableMethodParameter_DateTime_UseDefaultValue_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(default, service.GetDateTimeWithDefaultDefault());
        }

        [Theory]
        [MemberData(nameof(DateTimeCases))]
        public void OptionalNullableMethodParameter_DateTime_UseExplicitValue_Test(DateTime input)
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(input, service.GetDateTimeWithDefaultDefault(input));
        }
    }
}
