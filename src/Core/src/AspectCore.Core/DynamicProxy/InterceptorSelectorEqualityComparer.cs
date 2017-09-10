using System.Collections.Generic;

namespace AspectCore.DynamicProxy
{
    internal class InterceptorSelectorEqualityComparer : IEqualityComparer<IInterceptorSelector>
    {
        public bool Equals(IInterceptorSelector x, IInterceptorSelector y)
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

        public int GetHashCode(IInterceptorSelector obj)
        {
            return obj.GetType().GetHashCode();
        }
    }
}