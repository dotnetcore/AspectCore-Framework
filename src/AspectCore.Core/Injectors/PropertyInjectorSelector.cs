using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    public class PropertyInjectorSelector : IPropertyInjectorSelector
    {
        private static readonly ConcurrentDictionary<Type, IPropertyInjector[]> propertyInjectorCache = new ConcurrentDictionary<Type, IPropertyInjector[]>();

        public IPropertyInjector[] SelectPropertyInjector(Type interceptorType)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }
            return propertyInjectorCache.GetOrAdd(interceptorType, type => SelectPropertyInjectorCache(type).ToArray());
        }

        private IEnumerable<IPropertyInjector> SelectPropertyInjectorCache(Type type)
        {
            foreach (var property in type.GetTypeInfo().DeclaredProperties)
            {
                if (property.CanWrite && property.IsDefined(typeof(FromServicesAttribute)))
                {
                    yield return new PropertyInjector(
                        provider => provider.GetService(property.PropertyType),
                        new PropertyAccessor(property).CreatePropertySetter());
                }
            }
        }
    }
}
