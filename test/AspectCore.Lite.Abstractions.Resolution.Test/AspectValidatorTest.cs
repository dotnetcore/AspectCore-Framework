using AspectCore.Lite.Abstractions.Common;
using AspectCore.Lite.Abstractions.Resolution.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Resolution.Test
{
    public class AspectValidatorTest
    {
        public MethodInfo GetMethod<T>()
            where T : IValidatorModel
        {
            return MethodInfoHelpers.GetMethod<Action<T>>(m => m.Validate());
        }

        [Fact]
        public void ValidateDeclaringType_Test()
        {
            var validator = new AspectValidator(new AspectConfiguration());
            Assert.False(validator.Validate(GetMethod<NotPublicValidatorModel>()));
            Assert.False(validator.Validate(GetMethod<VauleTypeValidatorModel>()));
            Assert.False(validator.Validate(GetMethod<SealedValidatorModel>()));
        }

        [Fact]
        public void ValidateDeclaringMethod_Test()
        {
            var validator = new AspectValidator(new AspectConfiguration());
            Assert.False(validator.Validate(GetMethod<FinalMethodValidatorModel>()));
            Assert.False(validator.Validate(MethodInfoHelpers.GetMethod<Action<NonPublicMethodValidatorModel>>(m => m.Validate())));
            Assert.False(validator.Validate(MethodInfoHelpers.GetMethod<Action>(() => StaticMethodValidatorModel.Validate())));
        }

        [Fact]
        public void ValidateNonAspect_Test()
        {
            var validator = new AspectValidator(new AspectConfiguration());
            Assert.False(validator.Validate(GetMethod<ClassNonAspectValidatorModel>()));
            Assert.False(validator.Validate(GetMethod<MethodNonAspectValidatorModel>()));
        }

        [Fact]
        public void ValidateIgnoredList_Test()
        {
            var configuration = new AspectConfiguration();
            configuration.GetConfigurationOption<bool>().Add(m => m.DeclaringType.Name.Matches("IgnoredList*"));
            var validator = new AspectValidator(configuration);
            Assert.False(validator.Validate(GetMethod<IgnoredListValidatorModel>()));
            Assert.False(validator.Validate(MethodInfoHelpers.GetMethod<Action<object>>(m => m.ToString())));
        }

        [Fact]
        public void ValidateInterceptor_Test()
        {
            var validator = new AspectValidator(new AspectConfiguration());
            Assert.True(validator.Validate(GetMethod<ClassWithInterceptorValidatorModel>()));
            Assert.True(validator.Validate(GetMethod<MethodWithInterceptorValidatorModel>()));
        }
    }

    internal class NotPublicValidatorModel : IValidatorModel
    {
        public virtual void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public struct VauleTypeValidatorModel : IValidatorModel
    {
        public void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class SealedValidatorModel : IValidatorModel
    {
        public void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class StaticMethodValidatorModel
    {
        public static void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class FinalMethodValidatorModel : IValidatorModel
    {
        public void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class NonPublicMethodValidatorModel
    {
        internal virtual void Validate()
        {
            throw new NotImplementedException();
        }
    }

    [NonAspect]
    public class ClassNonAspectValidatorModel : IValidatorModel
    {
        public virtual void Validate()
        {
            throw new NotImplementedException();
        }
    }


    public class MethodNonAspectValidatorModel : IValidatorModel
    {
        [NonAspect]
        public virtual void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class IgnoredListValidatorModel : IValidatorModel
    {
        public virtual void Validate()
        {
            throw new NotImplementedException();
        }
    }

    [Increment]
    public class ClassWithInterceptorValidatorModel : IValidatorModel
    {
        public virtual void Validate()
        {
            throw new NotImplementedException();
        }
    }

   
    public class MethodWithInterceptorValidatorModel : IValidatorModel
    {
        [Increment]
        public virtual void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
