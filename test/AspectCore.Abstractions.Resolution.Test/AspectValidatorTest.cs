using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Resolution.Test.Fakes;
using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Abstractions.Resolution.Test
{
    public class AspectValidatorTest
    {
        [Fact]
        public void ValidateDeclaringType_Test()
        {
            var validator = new AspectValidator(new AspectConfiguration());
            Assert.False(validator.Validate(MethodExtensions.GetMethod<Action<NotPublicValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(MethodExtensions.GetMethod<Action<VauleTypeValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(MethodExtensions.GetMethod<Action<SealedValidatorModel>>(m => m.Validate())));
          
        }

        [Fact]
        public void ValidateDeclaringType_Type_Test()
        {
            var validator = new AspectValidator(new AspectConfiguration());
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
            var validator = new AspectValidator(new AspectConfiguration());
            Assert.False(validator.Validate(MethodExtensions.GetMethod<Action<FinalMethodValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(MethodExtensions.GetMethod<Action<NonPublicMethodValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(MethodExtensions.GetMethod<Action>(() => StaticMethodValidatorModel.Validate())));
        }

        [Fact]
        public void ValidateDeclaringMethod_Type_Test()
        {
            var validator = new AspectValidator(new AspectConfiguration());
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
            var validator = new AspectValidator(new AspectConfiguration());
            Assert.False(validator.Validate(MethodExtensions.GetMethod<Action<ClassNonAspectValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(MethodExtensions.GetMethod<Action<MethodNonAspectValidatorModel>>(m => m.Validate())));
        }

        [Fact]
        public void ValidateIgnoredList_Test()
        {
            var configuration = new AspectConfiguration();
            configuration.GetConfigurationOption<bool>().Add(m => m.DeclaringType.Name.Matches("IgnoredList*"));
            var validator = new AspectValidator(configuration);
            Assert.False(validator.Validate(MethodExtensions.GetMethod<Action<IgnoredListValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(MethodExtensions.GetMethod<Action<object>>(m => m.ToString())));
        }

        [Fact]
        public void ValidateInterceptor_Test()
        {
            var validator = new AspectValidator(new AspectConfiguration());
            Assert.True(validator.Validate(MethodExtensions.GetMethod<Action<ClassWithInterceptorValidatorModel>>(m => m.Validate())));
            Assert.True(validator.Validate(MethodExtensions.GetMethod<Action<MethodWithInterceptorValidatorModel>>(m => m.Validate())));
        }

        [Fact]
        public void ValidateInterceptor_Type_Test()
        {
            var validator = new AspectValidator(new AspectConfiguration());
            Assert.True(validator.Validate(typeof(ClassWithInterceptorValidatorModel)));
            Assert.True(validator.Validate(typeof(MethodWithInterceptorValidatorModel)));
            Assert.True(validator.Validate(typeof(ClassWithInterceptorValidatorModel).GetTypeInfo()));
            Assert.True(validator.Validate(typeof(MethodWithInterceptorValidatorModel).GetTypeInfo()));
        }

        [Fact]
        public void Configuration_ValidateInterceptor_Test()
        {
            var configuration = new AspectConfiguration();
            configuration.GetConfigurationOption<IInterceptor>().Add(m => new IncrementAttribute());
            var validator = new AspectValidator(configuration);
            Assert.True(validator.Validate(MethodExtensions.GetMethod<Action<ConfigurationInterceptorValidatorModel>>(m => m.Validate())));
        }
    }
}
