using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class PropertyReflector
    {
        internal static PropertyReflector Create(PropertyInfo reflectionInfo, CallOptions callOption)
        {
            if (reflectionInfo == null)
            {
                throw new ArgumentNullException(nameof(reflectionInfo));
            }
            return ReflectorCacheUtils<Tuple<PropertyInfo, CallOptions>, PropertyReflector>.GetOrAdd(Tuple.Create(reflectionInfo, callOption), CreateInternal);

            PropertyReflector CreateInternal(Tuple<PropertyInfo, CallOptions> item)
            {
                var property = item.Item1;
                if (property.DeclaringType.GetTypeInfo().ContainsGenericParameters)
                {
                    return new OpenGenericPropertyReflector(item.Item1);
                }
                if ((property.CanRead && property.GetMethod.IsStatic) || (property.CanWrite && property.SetMethod.IsStatic))
                {
                    return new StaticPropertyReflector(property);
                }
                if (property.DeclaringType.GetTypeInfo().IsValueType || item.Item2 == CallOptions.Call)
                {
                    return new CallPropertyReflector(property);
                }

                return new PropertyReflector(property);
            }
        }
    }
}
