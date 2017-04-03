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

        private readonly ModuleBuilder _moduleBuilder;
        private readonly AssemblyBuilder _assemblyBuilder;
        private readonly IDictionary<string, TypeInfo> createdTypeInfoCache = new Dictionary<string, TypeInfo>();

        internal ModuleBuilder ModuleBuilder
        {
            get { return _moduleBuilder; }
        }

        private ModuleGenerator()
        {
            var assemblyName = new AssemblyName($"{ProxyNameSpace}.{Guid.NewGuid()}");
            _assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule("main");
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

            return createdTypeInfoCache.GetOrAdd(typeName, valueFactory);
        }
    }
}