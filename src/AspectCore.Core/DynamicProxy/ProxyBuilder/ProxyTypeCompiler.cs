using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using AspectCore.DynamicProxy.ProxyBuilder.Builders;
using AspectCore.DynamicProxy.ProxyBuilder.Visitors;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy.ProxyBuilder
{
    internal class ProxyTypeCompiler
    {
        private const string ProxyNameSpace = "AspectCore.DynamicGenerated";
        private const string ProxyAssemblyName = "AspectCore.DynamicProxy.Generator";
        private readonly ModuleBuilder _moduleBuilder;
        private readonly Dictionary<string, Type> _definedTypes;
        private readonly object _lock = new object();
        private readonly ProxyNameUtils _proxyNameUtils;

        public ProxyTypeCompiler()
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(ProxyAssemblyName), AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = asmBuilder.DefineDynamicModule("core");
            _definedTypes = new Dictionary<string, Type>();
            _proxyNameUtils = new ProxyNameUtils();
        }

        internal Type CreateInterfaceProxy(Type interfaceType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            if (!interfaceType.GetTypeInfo().IsVisible() || !interfaceType.GetTypeInfo().IsInterface)
            {
                throw new InvalidOperationException($"Validate '{interfaceType}' failed because the type does not satisfy the visible conditions.");
            }

            lock (_lock)
            {
                var name = _proxyNameUtils.GetInterfaceImplTypeFullName(interfaceType);
                if (!_definedTypes.TryGetValue(name, out Type type))
                {
                    type = CreateInterfaceImplInternal(name, interfaceType, additionalInterfaces, aspectValidator);
                    _definedTypes[name] = type;
                }
                return type;
            }
        }

        internal Type CreateInterfaceProxy(Type interfaceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            if (!interfaceType.GetTypeInfo().IsVisible() || !interfaceType.GetTypeInfo().IsInterface)
            {
                throw new InvalidOperationException($"Validate '{interfaceType}' failed because the type does not satisfy the visible conditions.");
            }

            lock (_lock)
            {
                var name = _proxyNameUtils.GetProxyTypeName(interfaceType, implType);
                if (!_definedTypes.TryGetValue(name, out Type type))
                {
                    type = CreateInterfaceProxyInternal(name, interfaceType, implType, additionalInterfaces, aspectValidator);
                    _definedTypes[name] = type;
                }
                return type;
            }
        }

        internal Type CreateClassProxy(Type serviceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            if (!serviceType.GetTypeInfo().IsVisible() || !serviceType.GetTypeInfo().IsClass)
            {
                throw new InvalidOperationException($"Validate '{serviceType}' failed because the type does not satisfy the visible conditions.");
            }
            if (!implType.GetTypeInfo().CanInherited())
            {
                throw new InvalidOperationException($"Validate '{implType}' failed because the type does not satisfy the condition to be inherited.");
            }

            lock (_lock)
            {
                var name = _proxyNameUtils.GetProxyTypeName(serviceType, implType);
                if (!_definedTypes.TryGetValue(name, out Type type))
                {
                    type = CreateClassProxyInternal(name, serviceType, implType, additionalInterfaces, aspectValidator);
                    _definedTypes[name] = type;
                }
                return type;
            }
        }

        private Type CreateInterfaceImplInternal(string name, Type interfaceType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            // Phase 1: Build and compile the stub implementation type
            var builder = new InterfaceImplBuilder(name, null, interfaceType, additionalInterfaces, aspectValidator);
            var stubNode = builder.BuildStubOnly();

            var ctx = new ILEmitVisitorContext(_moduleBuilder);
            var visitor = new ILEmitVisitor(ctx);
            var stubType = visitor.VisitProxyType(stubNode);

            // Phase 2: Now compute proxy name using the compiled stub type, then build and compile proxy
            var proxyName = _proxyNameUtils.GetProxyTypeName(
                _proxyNameUtils.GetInterfaceImplTypeName(interfaceType), interfaceType, stubType);

            var proxyNode = builder.BuildProxyOnly(proxyName, stubType);

            // Use a fresh visitor for the proxy type (same module)
            var proxyVisitor = new ILEmitVisitor(new ILEmitVisitorContext(_moduleBuilder));
            return proxyVisitor.VisitProxyType(proxyNode);
        }

        private Type CreateInterfaceProxyInternal(string name, Type interfaceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            var builder = new InterfaceProxyBuilder(name, interfaceType, implType, additionalInterfaces, aspectValidator);
            var nodes = builder.Build();

            var visitor = new ILEmitVisitor(new ILEmitVisitorContext(_moduleBuilder));
            var types = visitor.VisitAll(nodes);

            return types[0];
        }

        private Type CreateClassProxyInternal(string name, Type serviceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            var builder = new ClassProxyBuilder(name, serviceType, implType, additionalInterfaces, aspectValidator);
            var nodes = builder.Build();

            var visitor = new ILEmitVisitor(new ILEmitVisitorContext(_moduleBuilder));
            var types = visitor.VisitAll(nodes);

            return types[0];
        }

        private class ProxyNameUtils
        {
            private readonly Dictionary<string, ProxyNameIndex> _indexs = new Dictionary<string, ProxyNameIndex>();
            private readonly Dictionary<Tuple<Type, Type>, string> _indexMaps = new Dictionary<Tuple<Type, Type>, string>();

            private string GetProxyTypeIndex(string className, Type serviceType, Type implementationType)
            {
                ProxyNameIndex nameIndex;
                if (!_indexs.TryGetValue(className, out nameIndex))
                {
                    nameIndex = new ProxyNameIndex();
                    _indexs[className] = nameIndex;
                }
                var key = Tuple.Create(serviceType, implementationType);
                string index;
                if (!_indexMaps.TryGetValue(key, out index))
                {
                    var tempIndex = nameIndex.GenIndex();
                    index = tempIndex == 0 ? string.Empty : tempIndex.ToString();
                    _indexMaps[key] = index;
                }
                Debug.WriteLine($"{className}-{serviceType}-{implementationType}-{index}");
                return index;
            }

            public string GetInterfaceImplTypeName(Type interfaceType)
            {
                var className = interfaceType.GetReflector().DisplayName;
                if (className.StartsWith("I", StringComparison.Ordinal))
                {
                    className = className.Substring(1);
                }
                return className;
            }

            public string GetInterfaceImplTypeFullName(Type interfaceType)
            {
                var className = GetInterfaceImplTypeName(interfaceType);
                return $"{ProxyNameSpace}.{className}{GetProxyTypeIndex(className, interfaceType, interfaceType)}";
            }

            public string GetProxyTypeName(Type serviceType, Type implType)
            {
                return $"{ProxyNameSpace}.{implType.GetReflector().DisplayName}{GetProxyTypeIndex(implType.GetReflector().DisplayName, serviceType, implType)}";
            }

            public string GetProxyTypeName(string className, Type serviceType, Type implType)
            {
                return $"{ProxyNameSpace}.{className}{GetProxyTypeIndex(className, serviceType, implType)}";
            }
        }

        private class ProxyNameIndex
        {
            private int _index = -1;

            public int GenIndex()
            {
                return Interlocked.Increment(ref _index);
            }
        }
    }
}
