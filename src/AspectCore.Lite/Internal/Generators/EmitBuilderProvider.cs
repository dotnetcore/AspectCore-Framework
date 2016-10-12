using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Generators
{
    internal sealed class EmitBuilderProvider
    {
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;
        private readonly ConcurrentDictionary<Type, Type> proxyTypes;

        public EmitBuilderProvider()
        {
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(GeneratorConstants.Assembly), AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(GeneratorConstants.Module);
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
