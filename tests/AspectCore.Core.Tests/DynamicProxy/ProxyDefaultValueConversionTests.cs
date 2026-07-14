using System;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyDefaultValueConversionTests : DynamicProxyTestBase
    {
        [Fact]
        public void ClassProxy_WithNullableIntAndIntDefault_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithNullableInt>((int?)5);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithLongAndIntDefault_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithLong>(5L);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithDoubleAndIntDefault_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithDouble>(5.0);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithDecimalAndIntDefault_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithDecimal>(5m);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithNullableEnumAndEnumDefault_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithNullableEnum>((TestEnum?)TestEnum.B);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithShortAndIntDefault_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithShort>((short)5);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithFloatAndIntDefault_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithFloat>(5f);
            Assert.NotNull(proxy);
        }

        public class CtorWithNullableInt
        {
            public CtorWithNullableInt(int? value = null) { }
            public virtual int GetValue() => 0;
        }

        public class CtorWithLong
        {
            public CtorWithLong(long value = 5) { }
            public virtual long GetValue() => 0;
        }

        public class CtorWithDouble
        {
            public CtorWithDouble(double value = 5) { }
            public virtual double GetValue() => 0;
        }

        public class CtorWithDecimal
        {
            public CtorWithDecimal(decimal value = 5) { }
            public virtual decimal GetValue() => 0;
        }

        public enum TestEnum { A, B, C }

        public class CtorWithNullableEnum
        {
            public CtorWithNullableEnum(TestEnum? value = null) { }
            public virtual TestEnum GetValue() => TestEnum.A;
        }

        public class CtorWithShort
        {
            public CtorWithShort(short value = 5) { }
            public virtual short GetValue() => 0;
        }

        public class CtorWithFloat
        {
            public CtorWithFloat(float value = 5) { }
            public virtual float GetValue() => 0;
        }
    }
}
