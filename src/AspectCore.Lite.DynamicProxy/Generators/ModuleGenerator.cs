using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    internal sealed class ModuleGenerator
    {
        
        private static readonly ModuleGenerator instance = new ModuleGenerator();
        internal static ModuleGenerator Default
        {
            get { return instance; }
        }

        private readonly ModuleBuilder moduleBuilder;
        private readonly ConcurrentDictionary<string, TypeInfo> createdTypeInfoPool;

        internal ModuleBuilder ModuleBuilder
        {
            get { return moduleBuilder; }
        }

        private ModuleGenerator()
        {
            createdTypeInfoPool = new ConcurrentDictionary<string, TypeInfo>();
            var assemblyName = new AssemblyName("AspectCore.Lite.Proxys");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("main");
        }

        internal TypeInfo DefineTypeInfo(string name, Func<string, TypeInfo> valueFactory)
        {
            return createdTypeInfoPool.GetOrAdd(name, valueFactory);
        }
    }
}
