using System;
using System.Collections;
using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    public class AspectValidationHandlerCollection: IEnumerable<IAspectValidationHandler>
    {
        private readonly HashSet<IAspectValidationHandler> _sets = new HashSet<IAspectValidationHandler>(new ValidationHandlerEqualityComparer());

        public AspectValidationHandlerCollection Add(IAspectValidationHandler aspectValidationHandler)
        {
            if (aspectValidationHandler == null)
            {
                throw new ArgumentNullException(nameof(aspectValidationHandler));
            }
            _sets.Add(aspectValidationHandler);
            return this;
        }
         
        public int Count => _sets.Count;

        public IEnumerator<IAspectValidationHandler> GetEnumerator()
        {
            return _sets.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class ValidationHandlerEqualityComparer : IEqualityComparer<IAspectValidationHandler>
        {
            public bool Equals(IAspectValidationHandler x, IAspectValidationHandler y)
            {
                if (x == null || y == null) return false;
                return x.GetType().Equals(y.GetType());
            }

            public int GetHashCode(IAspectValidationHandler obj)
            {
                return obj.GetType().GetHashCode();
            }
        }
    }
}