using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public sealed class NonAspectOptions
    {
        public Func<MethodInfo, bool> Predicate { get; }

        public NonAspectOptions(Func<MethodInfo, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            Predicate = predicate;
        }
    }
}