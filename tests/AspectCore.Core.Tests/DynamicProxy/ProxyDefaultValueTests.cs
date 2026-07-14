using System;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyDefaultValueTests : DynamicProxyTestBase
    {
        [Fact]
        public void ClassProxy_WithStringDefaultValue_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithStringDefault>("default");
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<CtorWithStringDefault>(proxy);
        }

        [Fact]
        public void ClassProxy_WithIntDefaultValue_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithIntDefault>(42);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithBoolDefaultValue_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithBoolDefault>(true);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithDoubleDefaultValue_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithDoubleDefault>(3.14);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithEnumDefaultValue_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithEnumDefault>(TestEnum.B);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithNullableDefaultValue_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithNullableDefault>((int?)5);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithMultipleDefaultValues_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithMultipleDefaults>("test", 10, false);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithDecimalDefaultValue_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithDecimalDefault>(1.5m);
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithCharDefaultValue_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithCharDefault>('a');
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithFloatDefaultValue_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CtorWithFloatDefault>(1.5f);
            Assert.NotNull(proxy);
        }

        public class CtorWithStringDefault
        {
            public CtorWithStringDefault(string name = "default") { }
            public virtual string GetName() => "test";
        }

        public class CtorWithIntDefault
        {
            public CtorWithIntDefault(int count = 42) { }
            public virtual int GetCount() => 0;
        }

        public class CtorWithBoolDefault
        {
            public CtorWithBoolDefault(bool flag = true) { }
            public virtual bool GetFlag() => false;
        }

        public class CtorWithDoubleDefault
        {
            public CtorWithDoubleDefault(double value = 3.14) { }
            public virtual double GetValue() => 0;
        }

        public enum TestEnum { A, B, C }

        public class CtorWithEnumDefault
        {
            public CtorWithEnumDefault(TestEnum e = TestEnum.B) { }
            public virtual TestEnum GetEnum() => TestEnum.A;
        }

        public class CtorWithNullableDefault
        {
            public CtorWithNullableDefault(int? value = null) { }
            public virtual int? GetValue() => null;
        }

        public class CtorWithMultipleDefaults
        {
            public CtorWithMultipleDefaults(string name = "test", int count = 10, bool flag = false) { }
            public virtual string GetName() => "test";
        }

        public class CtorWithDecimalDefault
        {
            public CtorWithDecimalDefault(decimal value = 1.5m) { }
            public virtual decimal GetValue() => 0;
        }

        public class CtorWithCharDefault
        {
            public CtorWithCharDefault(char c = 'a') { }
            public virtual char GetChar() => 'a';
        }

        public class CtorWithFloatDefault
        {
            public CtorWithFloatDefault(float value = 1.5f) { }
            public virtual float GetValue() => 0;
        }
    }
}
