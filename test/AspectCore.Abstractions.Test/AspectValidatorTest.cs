using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions.Internal;
using AspectCore.Abstractions.Internal.Test.Fakes;
using AspectCore.Abstractions.Test.Fakes;
using Xunit;

namespace AspectCore.Abstractions.Internal.Test
{
    public class AspectValidatorTest
    {
        [Fact]
        public void ValidateDeclaringType_Test()
        {
            var validator = AspectValidatorFactory.GetAspectValidator(new AspectConfigure());
            Assert.False(validator.Validate(ReflectionExtensions.GetMethod<Action<NotPublicValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(ReflectionExtensions.GetMethod<Action<VauleTypeValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(ReflectionExtensions.GetMethod<Action<SealedValidatorModel>>(m => m.Validate())));

        }

        [Fact]
        public void ValidateDeclaringType_Type_Test()
        {
            var validator = AspectValidatorFactory.GetAspectValidator(new AspectConfigure());
            Assert.False(validator.Validate(typeof(NotPublicValidatorModel)));
            Assert.False(validator.Validate(typeof(VauleTypeValidatorModel)));
            Assert.False(validator.Validate(typeof(SealedValidatorModel)));
            Assert.False(validator.Validate(typeof(NotPublicValidatorModel).GetTypeInfo()));
            Assert.False(validator.Validate(typeof(VauleTypeValidatorModel).GetTypeInfo()));
            Assert.False(validator.Validate(typeof(SealedValidatorModel).GetTypeInfo()));
        }

        [Fact]
        public void ValidateDeclaringMethod_Test()
        {
            var validator = AspectValidatorFactory.GetAspectValidator(new AspectConfigure());
            Assert.False(validator.Validate(ReflectionExtensions.GetMethod<Action<FinalMethodValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(ReflectionExtensions.GetMethod<Action<NonPublicMethodValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(ReflectionExtensions.GetMethod<Action>(() => StaticMethodValidatorModel.Validate())));
        }

        [Fact]
        public void ValidateDeclaringMethod_Type_Test()
        {
            var validator = AspectValidatorFactory.GetAspectValidator(new AspectConfigure());
            Assert.False(validator.Validate(typeof(FinalMethodValidatorModel)));
            Assert.False(validator.Validate(typeof(NonPublicMethodValidatorModel)));
            Assert.False(validator.Validate(typeof(StaticMethodValidatorModel)));
            Assert.False(validator.Validate(typeof(FinalMethodValidatorModel).GetTypeInfo()));
            Assert.False(validator.Validate(typeof(NonPublicMethodValidatorModel).GetTypeInfo()));
            Assert.False(validator.Validate(typeof(StaticMethodValidatorModel).GetTypeInfo()));
        }

        [Fact]
        public void ValidateNonAspect_Test()
        {
            var validator = AspectValidatorFactory.GetAspectValidator(new AspectConfigure());
            Assert.False(validator.Validate(ReflectionExtensions.GetMethod<Action<ClassNonAspectValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(ReflectionExtensions.GetMethod<Action<MethodNonAspectValidatorModel>>(m => m.Validate())));
        }

        [Fact]
        public void ValidateIgnoredList_Test()
        {
            var configure = new AspectConfigure();
            configure.GetConfigureOption<bool>().Add(m => m.DeclaringType.Name.Matches("IgnoredList*"));
            var validator = AspectValidatorFactory.GetAspectValidator(configure);
            Assert.False(validator.Validate(ReflectionExtensions.GetMethod<Action<IgnoredListValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(ReflectionExtensions.GetMethod<Action<object>>(m => m.ToString())));
        }

        [Fact]
        public void ValidateInterceptor_Test()
        {
            var validator = AspectValidatorFactory.GetAspectValidator(new AspectConfigure());
            Assert.True(validator.Validate(ReflectionExtensions.GetMethod<Action<ClassWithInterceptorValidatorModel>>(m => m.Validate())));
            Assert.True(validator.Validate(ReflectionExtensions.GetMethod<Action<MethodWithInterceptorValidatorModel>>(m => m.Validate())));
        }

        [Fact]
        public void ValidateInterceptor_Type_Test()
        {
            var validator = AspectValidatorFactory.GetAspectValidator(new AspectConfigure());
            Assert.True(validator.Validate(typeof(ClassWithInterceptorValidatorModel)));
            Assert.True(validator.Validate(typeof(MethodWithInterceptorValidatorModel)));
            Assert.True(validator.Validate(typeof(ClassWithInterceptorValidatorModel).GetTypeInfo()));
            Assert.True(validator.Validate(typeof(MethodWithInterceptorValidatorModel).GetTypeInfo()));
        }

        [Fact]
        public void Configure_ValidateInterceptor_Test()
        {
            var configure = new AspectConfigure();
            configure.GetConfigureOption<IInterceptor>().Add(m => new IncrementAttribute());
            var validator = AspectValidatorFactory.GetAspectValidator(configure);
            Assert.True(validator.Validate(ReflectionExtensions.GetMethod<Action<ConfigureInterceptorValidatorModel>>(m => m.Validate())));
        }
    }
}
