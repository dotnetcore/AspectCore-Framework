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
                if ((property.CanRead && property.GetMethod.IsStatic) || (property.CanWrite && property.SetMethod.IsStatic) || property.DeclaringType.GetTypeInfo().IsValueType)
                {
                    return new StaticPropertyReflector(property);
                }

                return new PropertyReflector(property, item.Item2);
            }
        }
    }
}