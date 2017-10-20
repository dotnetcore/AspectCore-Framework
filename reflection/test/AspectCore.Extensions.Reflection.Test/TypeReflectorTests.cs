using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class TypeReflectorTests
    {
        [Fact]
        public void Type_DisplayName()
        {
            var displayName = typeof(FakeClass).GetTypeInfo().GetReflector().DisplayName;
            Assert.Equal("FakeClass", displayName);
        }

        [Fact]
        public void Nested_Type_DisplayName()
        {
            var displayName = typeof(FakeClass.FakeNestedClass).GetTypeInfo().GetReflector().DisplayName;
            var ss = typeof(FakeClass.FakeNestedClass).FullName;
            Assert.Equal("FakeClass.FakeNestedClass", displayName);
        }

        [Fact]
        public void OpenGeneric_Type_DisplayName()
        {
            var displayName = typeof(FakeGenericClass<,>).GetTypeInfo().GetReflector().DisplayName;
            Assert.Equal("FakeGenericClass<K,V>", displayName);
        }

        [Fact]
        public void CloseGeneric_Type_DisplayName()
        {
            var displayName = typeof(FakeGenericClass<FakeGenericClass<string,int>, FakeClass>).GetTypeInfo().GetReflector().DisplayName;
            Assert.Equal("FakeGenericClass<FakeGenericClass<String,Int32>,FakeClass>", displayName);
        }
    }

    public class FakeClass {
        public class FakeNestedClass { }
    }

    public class FakeGenericClass<K, V>
    {

    }
}
