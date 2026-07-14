using System;
using System.Reflection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectValidationContextTests
    {
        private static MethodInfo GetMethod(string name) => typeof(TestService).GetMethod(name);

        #region Default Values

        [Fact]
        public void Default_StrictValidation_IsFalse()
        {
            var context = new AspectValidationContext();
            Assert.False(context.StrictValidation);
        }

        [Fact]
        public void Default_Method_IsNull()
        {
            var context = new AspectValidationContext();
            Assert.Null(context.Method);
        }

        #endregion

        #region Property Set/Get

        [Fact]
        public void Method_CanBeSetAndRetrieved()
        {
            var method = GetMethod(nameof(TestService.Foo));
            var context = new AspectValidationContext { Method = method };
            Assert.Same(method, context.Method);
        }

        [Fact]
        public void StrictValidation_CanBeSetAndRetrieved()
        {
            var context = new AspectValidationContext { StrictValidation = true };
            Assert.True(context.StrictValidation);
        }

        [Fact]
        public void StrictValidation_CanBeToggled()
        {
            var context = new AspectValidationContext();
            Assert.False(context.StrictValidation);
            context.StrictValidation = true;
            Assert.True(context.StrictValidation);
            context.StrictValidation = false;
            Assert.False(context.StrictValidation);
        }

        #endregion

        #region Equals (IEquatable<AspectValidationContext>)

        [Fact]
        public void Equals_SameValues_ReturnsTrue()
        {
            var method = GetMethod(nameof(TestService.Foo));
            var context1 = new AspectValidationContext { Method = method, StrictValidation = true };
            var context2 = new AspectValidationContext { Method = method, StrictValidation = true };
            Assert.True(context1.Equals(context2));
        }

        [Fact]
        public void Equals_DifferentMethod_ReturnsFalse()
        {
            var method1 = GetMethod(nameof(TestService.Foo));
            var method2 = GetMethod(nameof(TestService.Bar));
            var context1 = new AspectValidationContext { Method = method1, StrictValidation = true };
            var context2 = new AspectValidationContext { Method = method2, StrictValidation = true };
            Assert.False(context1.Equals(context2));
        }

        [Fact]
        public void Equals_DifferentStrictValidation_ReturnsFalse()
        {
            var method = GetMethod(nameof(TestService.Foo));
            var context1 = new AspectValidationContext { Method = method, StrictValidation = true };
            var context2 = new AspectValidationContext { Method = method, StrictValidation = false };
            Assert.False(context1.Equals(context2));
        }

        [Fact]
        public void Equals_BothNullMethod_ReturnsTrue()
        {
            var context1 = new AspectValidationContext { StrictValidation = false };
            var context2 = new AspectValidationContext { StrictValidation = false };
            Assert.True(context1.Equals(context2));
        }

        [Fact]
        public void Equals_OneNullMethod_ReturnsFalse()
        {
            var method = GetMethod(nameof(TestService.Foo));
            var context1 = new AspectValidationContext { Method = method, StrictValidation = false };
            var context2 = new AspectValidationContext { StrictValidation = false };
            Assert.False(context1.Equals(context2));
        }

        #endregion

        #region Equals (object)

        [Fact]
        public void Equals_Object_SameValues_ReturnsFalseDueToTypeCheckBug()
        {
            // Note: AspectValidationContext.Equals(object) incorrectly checks
            // 'obj is AspectActivatorContext' instead of 'obj is AspectValidationContext',
            // so it returns false even when the values are the same.
            var method = GetMethod(nameof(TestService.Foo));
            var context1 = new AspectValidationContext { Method = method, StrictValidation = true };
            object context2 = new AspectValidationContext { Method = method, StrictValidation = true };
            Assert.False(context1.Equals(context2));
        }

        [Fact]
        public void Equals_Object_NullObject_ReturnsFalse()
        {
            var context = new AspectValidationContext();
            Assert.False(context.Equals(null));
        }

        [Fact]
        public void Equals_Object_DifferentType_ReturnsFalse()
        {
            var context = new AspectValidationContext();
            Assert.False(context.Equals("not a context"));
        }

        #endregion

        #region GetHashCode

        [Fact]
        public void GetHashCode_SameValues_ReturnsSameHashCode()
        {
            var method = GetMethod(nameof(TestService.Foo));
            var context1 = new AspectValidationContext { Method = method, StrictValidation = true };
            var context2 = new AspectValidationContext { Method = method, StrictValidation = true };
            Assert.Equal(context1.GetHashCode(), context2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_DefaultValues_ReturnsConsistentHashCode()
        {
            var context1 = new AspectValidationContext();
            var context2 = new AspectValidationContext();
            Assert.Equal(context1.GetHashCode(), context2.GetHashCode());
        }

        #endregion

        #region IsStruct

        [Fact]
        public void IsStruct()
        {
            Assert.True(typeof(AspectValidationContext).IsValueType);
        }

        #endregion

        #region Test Types

        private class TestService
        {
            public virtual void Foo() { }

            public virtual int Bar(int value) => value;
        }

        #endregion
    }
}
