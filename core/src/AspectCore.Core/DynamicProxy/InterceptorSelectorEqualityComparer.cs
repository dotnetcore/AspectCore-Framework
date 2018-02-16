using System.Collections.Generic;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    internal class InterceptorSelectorEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public bool Equals(T x, T y)
        {
            if (x == null || y == null)
            {
                return false;
            }
            if (x == y)
            {
                return true;
            }
            if (x.GetType() == y.GetType())
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(T obj)
        {
            return obj.GetType().GetHashCode();
        }
    }
}