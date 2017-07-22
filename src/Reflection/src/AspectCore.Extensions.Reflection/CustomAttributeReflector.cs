using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Reflection
{
    public sealed class CustomAttributeReflector
    {
        private CustomAttributeReflector(CustomAttributeData reflectionInfo)
        {
        }

        #region internal
        internal static CustomAttributeReflector Create(CustomAttributeData reflectionInfo)
        {
            if (reflectionInfo == null)
            {
                throw new ArgumentNullException(nameof(reflectionInfo));
            }
            return ReflectorCache<CustomAttributeData, CustomAttributeReflector>.GetOrAdd(reflectionInfo, info => new CustomAttributeReflector(reflectionInfo));
        }
        #endregion
    }
}
