using System;

namespace AspectCore.Abstractions.Resolution.Test.Fakes
{
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

    public class ConfigurationInterceptorValidatorModel : IValidatorModel
    {
        public virtual void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
