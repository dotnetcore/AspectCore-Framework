using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Internal.Generators
{
    [NonAspect]
    internal sealed class ModuleGenerator
    {
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly ModuleBuilder moduleBuilder;
        private readonly ConcurrentDictionary<Type, Type> proxyTypes;

        public ModuleGenerator()
        {
#if NET45
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(GeneratorConstants.Assembly), AssemblyBuilderAccess.RunAndSave);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(GeneratorConstants.Module , GeneratorConstants.AssemblyFile);
#else
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(GeneratorConstants.Assembly), AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule(GeneratorConstants.Module);
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

#if NET45
        public void SaveAssembly()
        {
            assemblyBuilder.Save(GeneratorConstants.AssemblyFile);
        }
#endif
    }
}
