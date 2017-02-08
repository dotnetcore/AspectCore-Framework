using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions.Extensions;

namespace AspectCore.Abstractions.Internal.Generator
{
    internal sealed class ModuleGenerator
    {
        internal const string ProxyNameSpace = "AspectCore.Proxies";

        private static readonly ModuleGenerator instance = new ModuleGenerator();
        internal static ModuleGenerator Default
        {
            get { return instance; }
        }

        private readonly ModuleBuilder moduleBuilder;
        private readonly AssemblyBuilder assemblyBuilder;
        private readonly IDictionary<string, TypeInfo> createdTypeInfoCache = new Dictionary<string, TypeInfo>();
        private readonly object cacheLock = new object();

        internal ModuleBuilder ModuleBuilder
        {
            get { return moduleBuilder; }
        }

        private ModuleGenerator()
        {
            var assemblyName = new AssemblyName($"{ProxyNameSpace}.{Guid.NewGuid()}");
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