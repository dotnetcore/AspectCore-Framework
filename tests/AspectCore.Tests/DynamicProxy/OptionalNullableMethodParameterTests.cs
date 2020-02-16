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
    public class OptionalNullableMethodParameterTests : DynamicProxyTestBase
    {
        public static IEnumerable<object[]> DecimalCases { get; } =
            new decimal?[] { 1.23m, 1, 0, -1, null }.Select(m => new object[] { m });

        public static IEnumerable<object[]> DateTimeCases { get; } =
            new DateTime?[] { default(DateTime), new DateTime(2000, 1, 1), null }.Select(m => new object[] { m });

        public class Interceptor : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return context.Invoke(next);
            }
        }

        public class Service
        {
            [Interceptor] public virtual Bool? GetEnumWithDefaultTrue(Bool? value = Bool.True) => value;
            [Interceptor] public virtual Bool? GetEnumWithDefaultFalse(Bool? value = Bool.False) => value;
            [Interceptor] public virtual Bool? GetEnumWithDefaultNull(Bool? value = null) => value;
            [Interceptor] public virtual int? GetIntWithDefaultNull(int? value = null) => value;
            [Interceptor] public virtual int? GetIntWithDefaultOne(int? value = 1) => value;
            [Interceptor] public virtual string GetStringWithDefaultNull(string value = null) => value;
            [Interceptor] public virtual string GetStringWithDefaultEmpty(string value = "") => value;
            [Interceptor] public virtual decimal? GetDecimalWithDefaultNull(decimal? value = null) => value;
            [Interceptor] public virtual decimal? GetDecimalWithDefaultOne(decimal? value = 1) => value;
            [Interceptor] public virtual DateTime? GetDateTimeWithDefaultNull(DateTime? value = null) => value;
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
            Assert.Null(service.GetEnumWithDefaultNull());
        }

        [Theory]
        [InlineData(Bool.True)]
        [InlineData(Bool.False)]
        [InlineData(null)]
        public void OptionalNullableMethodParameter_Enum_UseExplicitValue_Test(Bool? input)
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(input, service.GetEnumWithDefaultTrue(input));
            Assert.Equal(input, service.GetEnumWithDefaultFalse(input));
            Assert.Equal(input, service.GetEnumWithDefaultNull(input));
        }

        [Fact]
        public void OptionalNullableMethodParameter_Int_UseDefaultValue_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(1, service.GetIntWithDefaultOne());
            Assert.Null(service.GetIntWithDefaultNull());
        }

        [Theory]
        [InlineData(1)]
        [InlineData(0)]
        [InlineData(null)]
        public void OptionalNullableMethodParameter_Int_UseExplicitValue_Test(int? input)
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(input, service.GetIntWithDefaultOne(input));
            Assert.Equal(input, service.GetIntWithDefaultNull(input));
        }

        [Fact]
        public void OptionalNullableMethodParameter_String_UseDefaultValue_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal("", service.GetStringWithDefaultEmpty());
            Assert.Null(service.GetStringWithDefaultNull());
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("")]
        [InlineData(null)]
        public void OptionalNullableMethodParameter_String_UseExplicitValue_Test(string input)
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(input, service.GetStringWithDefaultEmpty(input));
            Assert.Equal(input, service.GetStringWithDefaultNull(input));
        }

        [Fact]
        public void OptionalNullableMethodParameter_Decimal_UseDefaultValue_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(1, service.GetDecimalWithDefaultOne());
            Assert.Null(service.GetDecimalWithDefaultNull());
        }

        [Theory]
        [MemberData(nameof(DecimalCases))]
        public void OptionalNullableMethodParameter_Decimal_UseExplicitValue_Test(decimal? input)
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(input, service.GetDecimalWithDefaultOne(input));
            Assert.Equal(input, service.GetDecimalWithDefaultNull(input));
        }


        [Fact]
        public void OptionalNullableMethodParameter_DateTime_UseDefaultValue_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(default, service.GetDateTimeWithDefaultNull());
        }

        [Theory]
        [MemberData(nameof(DateTimeCases))]
        public void OptionalNullableMethodParameter_DateTime_UseExplicitValue_Test(DateTime? input)
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            Assert.Equal(input, service.GetDateTimeWithDefaultNull(input));
        }
    }
}
