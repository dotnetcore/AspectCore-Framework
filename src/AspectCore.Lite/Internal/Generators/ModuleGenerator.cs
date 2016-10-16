using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Generators
{
    public sealed class ModuleGenerator
    {
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;
        private readonly ConcurrentDictionary<Type, Type> proxyTypes;

        public ModuleGenerator()
        {
#if NETSTANDARD1_6
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(GeneratorConstants.Assembly) , AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(GeneratorConstants.Module);
#elif NET451
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(GeneratorConstants.Assembly), AssemblyBuilderAccess.RunAndSave);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(GeneratorConstants.Module , GeneratorConstants.AssemblyFile);
#endif

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

#if NET451
        public void SaveAssembly()
        {
            assemblyBuilder.Save(GeneratorConstants.AssemblyFile);
        }
#endif
    }
}
