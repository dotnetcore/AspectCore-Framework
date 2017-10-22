using System.Reflection;
using System;

namespace AspectCore.DynamicProxy
{
    public struct AspectValidationContext : IEquatable<AspectValidationContext>
    {
        public MethodInfo Method { get; set; }

        public bool StrictValidation { get; set; }

        public bool Equals(AspectValidationContext other)
        {
            return Method == other.Method && StrictValidation == other.StrictValidation;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is AspectActivatorContext other)
                return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            var hash_1 = this.Method?.GetHashCode() ?? 0;
            var hash_2 = StrictValidation.GetHashCode();
            return (hash_1 << 5) + hash_1 ^ hash_2;
        }
    }
}