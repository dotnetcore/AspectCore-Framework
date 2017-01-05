using AspectCore.Lite.Abstractions.Resolution.Generators;
using System;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Resolution
{
    public sealed class TypeGeneratorWrapper
    {
        public TypeInfo CreateTypeInfo(Type serviceType, Type implementationType, IAspectValidator aspectValidator)
        {
            var typeGenerator = new AspectTypeGenerator(serviceType, implementationType, aspectValidator);
            return typeGenerator.CreateTypeInfo();
        }

        public Type CreateType(Type serviceType, Type implementationType, IAspectValidator aspectValidator)
        {
            var typeGenerator = new AspectTypeGenerator(serviceType, implementationType, aspectValidator);
            return typeGenerator.CreateType();
        }
    }
}
