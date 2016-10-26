using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Internal.Generators
{
    internal sealed class ModuleGenerator
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
            ExceptionHelper.ThrowArgumentNull(targetType , nameof(valueFactory));
            ExceptionHelper.ThrowArgumentNull(valueFactory , nameof(valueFactory));

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
