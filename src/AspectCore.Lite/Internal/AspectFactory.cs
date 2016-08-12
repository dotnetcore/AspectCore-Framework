
using AspectCore.Lite.Core;
using AspectCore.Lite.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Internal
{
    internal class AspectFactory : IAspectFactory
    {
        public Aspect Create(Type adviceType, IPointcut pointcut)
        {
            if (adviceType == null) throw new ArgumentNullException(nameof(adviceType));
            if (pointcut == null) throw new ArgumentNullException(nameof(pointcut));

            TypeInfo adviceTypeInfo = adviceType.GetTypeInfo();

            if (adviceTypeInfo.IsAbstract || adviceTypeInfo.IsInterface)
                throw new ArgumentException($"Type {adviceType.Name} cannot be abstract class or interface.", nameof(adviceType));

            if (!typeof(IAdvice).GetTypeInfo().IsAssignableFrom(adviceTypeInfo))
                throw new ArgumentException($"Type {adviceType.Name} cannot create an instance of IAdvice.", nameof(adviceType));

            return new Aspect(adviceType, pointcut);
        }

        public Aspect Create(IAdvice advice, IPointcut pointcut)
        {
            if (advice == null) throw new ArgumentNullException(nameof(advice));
            if (pointcut == null) throw new ArgumentNullException(nameof(pointcut));

            return new Aspect(advice, pointcut);
        }
    }
}
