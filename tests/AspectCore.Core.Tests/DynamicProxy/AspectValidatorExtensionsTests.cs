using System;
using System.Reflection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectValidatorExtensionsTests
    {
        [Fact]
        public void Validate_WithNullAspectValidator_ThrowsArgumentNullException()
        {
            IAspectValidator validator = null;
            var ex = Assert.Throws<ArgumentNullException>(() => validator.Validate(typeof(string), false));
            Assert.Equal("aspectValidator", ex.ParamName);
        }

        [Fact]
        public void Validate_WithNullType_ReturnsFalse()
        {
            var validator = new FakeAspectValidator(true);
            Assert.False(AspectValidatorExtensions.Validate(validator, null, false));
        }

        [Fact]
        public void Validate_WithValueType_ReturnsFalse()
        {
            var validator = new FakeAspectValidator(true);
            Assert.False(validator.Validate(typeof(int), false));
        }

        [Fact]
        public void Validate_WithEnum_ReturnsFalse()
        {
            var validator = new FakeAspectValidator(true);
            Assert.False(validator.Validate(typeof(DayOfWeek), false));
        }

        [Fact]
        public void Validate_WithNonVisibleType_ReturnsFalse()
        {
            var validator = new FakeAspectValidator(true);
            Assert.False(validator.Validate(typeof(InternalPrivateType), false));
        }

        [Fact]
        public void Validate_WithNonAspectType_ReturnsFalse()
        {
            var validator = new FakeAspectValidator(true);
            Assert.False(validator.Validate(typeof(NonAspectType), false));
        }

        [Fact]
        public void Validate_WithSealedClass_ReturnsFalse()
        {
            var validator = new FakeAspectValidator(true);
            Assert.False(validator.Validate(typeof(SealedClass), false));
        }

        [Fact]
        public void Validate_WithValidatableMethod_ReturnsTrue()
        {
            var validator = new FakeAspectValidator(true);
            Assert.True(validator.Validate(typeof(ValidatableClass), false));
        }

        [Fact]
        public void Validate_WhenMethodValidationFails_ChecksInterfaces()
        {
            var validator = new FakeAspectValidator(false, true);
            Assert.True(validator.Validate(typeof(ImplWithInterface), false));
        }

        [Fact]
        public void Validate_WhenMethodAndInterfaceValidationFails_ChecksBaseType()
        {
            var validator = new FakeAspectValidator(false, false, true);
            Assert.True(validator.Validate(typeof(DerivedClass), false));
        }

        [Fact]
        public void Validate_WhenNothingValidates_ReturnsFalse()
        {
            var validator = new FakeAspectValidator(false);
            Assert.False(validator.Validate(typeof(ValidatableClass), false));
        }

        [Fact]
        public void Validate_WithInterfaceType_ReturnsTrueWhenMethodValidates()
        {
            var validator = new FakeAspectValidator(true);
            Assert.True(validator.Validate(typeof(ITestInterface), false));
        }

        private class FakeAspectValidator : IAspectValidator
        {
            private readonly bool _methodResult;
            private readonly bool _interfaceMethodResult;
            private readonly bool _baseTypeMethodResult;
            private int _callCount;

            public FakeAspectValidator(bool methodResult, bool interfaceMethodResult = false, bool baseTypeMethodResult = false)
            {
                _methodResult = methodResult;
                _interfaceMethodResult = interfaceMethodResult;
                _baseTypeMethodResult = baseTypeMethodResult;
            }

            public bool Validate(MethodInfo method, bool isStrictValidation)
            {
                _callCount++;
                // First calls are for methods on the type itself, then interfaces, then base type
                if (_callCount <= 3)
                    return _methodResult;
                if (_callCount <= 6)
                    return _interfaceMethodResult;
                return _baseTypeMethodResult;
            }
        }

        private class InternalPrivateType { }

        [NonAspect]
        public class NonAspectType { }

        public sealed class SealedClass { }

        public class ValidatableClass
        {
            public virtual void DoSomething() { }
        }

        public interface ITestInterface
        {
            void DoSomething();
        }

        public class ImplWithInterface : ITestInterface
        {
            public void DoSomething() { }
        }

        public class BaseClass
        {
            public virtual void DoSomething() { }
        }

        public class DerivedClass : BaseClass
        {
            public new void DoSomething() { }
        }
    }
}
