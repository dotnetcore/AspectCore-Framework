using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions.Extensions;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal sealed class ModuleGenerator
    {
        private static readonly ModuleGenerator instance = new ModuleGenerator();
        internal static ModuleGenerator Default
        {
            get { return instance; }
        }

        private readonly ModuleBuilder moduleBuilder;
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly IDictionary<string, TypeInfo> createdTypeInfoCache;
        private readonly object cacheLock = new object();

        internal ModuleBuilder ModuleBuilder
        {
            get { return moduleBuilder; }
        }

        private ModuleGenerator()
        {
            createdTypeInfoCache = new Dictionary<string, TypeInfo>();
            var assemblyName = new AssemblyName("AspectCore.Proxys");
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = assemblyBuilder.DefineDynamicModule("main");
        }

        internal TypeInfo DefineTypeInfo(string typeName, Func<string, TypeInfo> valueFactory)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }

            return createdTypeInfoCache.GetOrAdd(typeName, valueFactory, cacheLock);
        }
    }
}
