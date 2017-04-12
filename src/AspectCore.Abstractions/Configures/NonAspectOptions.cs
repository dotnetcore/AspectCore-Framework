using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public sealed class NonAspectOptions
    {
        public Predicate<MethodInfo> Predicate { get; }

        public NonAspectOptions(Predicate<MethodInfo> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            Predicate = predicate;
        }
    }
}
