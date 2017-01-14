using AspectCore.Lite.Abstractions.Extensions;
using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test
{
    public class CanInheritedTest: NestedCanInheritedModel
    {
        [Theory]
        [InlineData(typeof(InterfaceCanInheritedModel))]
        [InlineData(typeof(SealedCanInheritedModel))]
        [InlineData(typeof(VauleTypeCanInheritedModel))]
        [InlineData(typeof(NonAspectCanInheritedModel))]
        [InlineData(typeof(DynamicallyCanInheritedModel))]
        [InlineData(typeof(NonPublicCanInheritedModel))]
        [InlineData(typeof(NestedCanInheritedModel.NestedDynamicallyCanInheritedModel))]
        [InlineData(typeof(NestedCanInheritedModel.NestedInterfaceCanInheritedModel))]
        [InlineData(typeof(NestedCanInheritedModel.NestedNonAspectCanInheritedModel))]
        [InlineData(typeof(NestedCanInheritedModel.NestedNonPublicCanInheritedModel))]
        [InlineData(typeof(NestedCanInheritedModel.NestedSealedCanInheritedModel))]
        [InlineData(typeof(NestedVauleTypeCanInheritedModel))]      
        [InlineData(typeof(NestedprivateCanInheritedModel))]
        [InlineData(typeof(NonPublicNestedCanInheritedModel.NestedPublicCanInheritedModel))]
        public void CanInherited_Test(Type type)
        {
            Assert.False(type.GetTypeInfo().CanInherited());
        }
    }

    public interface InterfaceCanInheritedModel { }

    public sealed class SealedCanInheritedModel { }

    public struct VauleTypeCanInheritedModel { }

    [NonAspect]
    public class NonAspectCanInheritedModel { }

    [Dynamically]
    public class DynamicallyCanInheritedModel { }

    internal class NonPublicCanInheritedModel { }

    public class NestedCanInheritedModel
    {
        public interface NestedInterfaceCanInheritedModel { }

        public sealed class NestedSealedCanInheritedModel { }

        public struct NestedVauleTypeCanInheritedModel { }

        [NonAspect]
        public class NestedNonAspectCanInheritedModel { }

        [Dynamically]
        public class NestedDynamicallyCanInheritedModel { }

        internal class NestedNonPublicCanInheritedModel { }

        protected class NestedprivateCanInheritedModel { }
    }

    internal class NonPublicNestedCanInheritedModel
    {
        public class NestedPublicCanInheritedModel { }
    }
}
