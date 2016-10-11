using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Generators
{
    internal sealed class EmitBuilderProvider
    {
        private const string assemblyName = "AspectCore.Lite.Runtime$Proxys";
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;
        private readonly ConcurrentDictionary<Type, Type> proxyTypes;

        public EmitBuilderProvider()
        {
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("main");
            proxyTypes = new ConcurrentDictionary<Type, Type>();
        }

        internal AssemblyBuilder CurrentAssemblyBuilder => assemblyBuilder;

        internal ModuleBuilder CurrentModuleBuilder => moduleBuilder;

        internal Type DefinedType(Type targetType, Func<Type, Type> valueFactory)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }
            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }
            return proxyTypes.GetOrAdd(targetType, valueFactory);
        }
    }
}
