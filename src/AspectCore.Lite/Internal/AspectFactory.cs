
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
        public Aspect Create(Type interceptorType, IPointcut pointcut)
        {
            if (interceptorType == null) throw new ArgumentNullException(nameof(interceptorType));
            if (pointcut == null) throw new ArgumentNullException(nameof(pointcut));

            TypeInfo interceptorTypeInfo = interceptorType.GetTypeInfo();

            if (interceptorTypeInfo.IsAbstract || interceptorTypeInfo.IsInterface)
                throw new ArgumentException($"Type {interceptorType.Name} cannot be abstract class or interface.", nameof(interceptorType));

            if (!typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(interceptorTypeInfo))
                throw new ArgumentException($"Type {interceptorType.Name} cannot create an instance of IInterceptor." , nameof(interceptorType));

            return new Aspect(interceptorType , pointcut);
        }

        public Aspect Create(IInterceptor interceptor , IPointcut pointcut)
        {
            if (interceptor == null) throw new ArgumentNullException(nameof(interceptor));
            if (pointcut == null) throw new ArgumentNullException(nameof(pointcut));

            return new Aspect(interceptor , pointcut);
        }
    }
}
