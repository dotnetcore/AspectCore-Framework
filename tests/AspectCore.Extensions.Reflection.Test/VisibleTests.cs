using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class VisibleTests
    {
        [Fact]
        public void Type_Visible()
        {
            Assert.True(typeof(FakePublicVisible).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeNonPublicVisible).GetTypeInfo().IsVisible());
        }

        [Fact]
        public void GenericType_Visible()
        {
            Assert.True(typeof(FakeGenericVisible<FakePublicVisible>).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeGenericVisible<FakeNonPublicVisible>).GetTypeInfo().IsVisible());
            Assert.True(typeof(FakeGenericVisible<FakePublicVisible.FakePublicNestedVisible>).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeGenericVisible<FakePublicVisible.FakeNonPublicNestedVisible>).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeGenericVisible<FakeNonPublicVisible.FakePublicNestedVisible>).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeGenericVisible<FakePublicVisible.FakeNonPublicNestedVisible>).GetTypeInfo().IsVisible());
        }

        [Fact]
        public void NestedType_Visible()
        {
            Assert.True(typeof(FakePublicVisible.FakePublicNestedVisible).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakePublicVisible.FakeNonPublicNestedVisible).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeNonPublicVisible.FakePublicNestedVisible).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakePublicVisible.FakeNonPublicNestedVisible).GetTypeInfo().IsVisible());
        }
    }

    public class FakePublicVisible
    {
        public class FakePublicNestedVisible { }
        internal class FakeNonPublicNestedVisible { }
    }

    internal class FakeNonPublicVisible
    {
        public class FakePublicNestedVisible { }
        internal class FakeNonPublicNestedVisible { }
    }

    public class FakeGenericVisible<T>
    {
    }
}