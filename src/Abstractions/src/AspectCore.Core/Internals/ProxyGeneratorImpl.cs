using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Core.Internal
{
    internal class ProxyGeneratorImpl
    {
        private const string ProxyNameSpace = "AspectCore.Core.ProxyBuilder";
        private static readonly ModuleBuilder _moduleBuilder;
        private static readonly Dictionary<string, Type> _definedTypes;

        static ProxyGeneratorImpl()
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(ProxyNameSpace), AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = asmBuilder.DefineDynamicModule("core");
            _definedTypes = new Dictionary<string, Type>();
        }

        private readonly IAspectValidator _aspectValidator;

        internal ProxyGeneratorImpl(IAspectValidator aspectValidator)
        {
            _aspectValidator = aspectValidator;
        }

        internal Type CreateInterfaceProxy(Type interfaceType, Type implType, Type[] additionalInterfaces)
        {
            var name = GetProxyTypeName();

            Type proxyType;
            if (_definedTypes.TryGetValue(name, out proxyType))
            {
                return proxyType;
            }

            lock (_moduleBuilder)
            {
                if (_definedTypes.TryGetValue(name, out proxyType))
                {
                    return proxyType;
                }
                proxyType = CreateInterfaceProxyInternal(name, interfaceType, implType, additionalInterfaces);
                _definedTypes.Add(name, proxyType);
                return proxyType;
            }

            string GetProxyTypeName()
            {
                return $"{ProxyNameSpace}.{implType.Name}Proxy^{interfaceType.Name}";
            }
        }

        private Type CreateInterfaceProxyInternal(string name, Type interfaceType, Type implType, Type[] additionalInterfaces)
        {
            var interfaces = new Type[] { interfaceType }.Concat(additionalInterfaces).Distinct().ToArray();

            //define proxy type for interface service
            var typeBuilder = _moduleBuilder.DefineType(name, TypeAttributes.Class | TypeAttributes.Public, typeof(object), interfaces);

            //define genericParameter
            if (interfaceType.GetTypeInfo().IsGenericType)
            {
                GenericParameterHelpers.DefineGenericParameter(interfaceType, typeBuilder);
            }

            //define private field
            var fieldTable = FieldBuilderHelpers.DefineInterfaceProxyField(interfaceType, typeBuilder);

            //define constructor
            ConstructorBuilderHelpers.DefineInterfaceProxyConstructor(interfaceType, typeBuilder, fieldTable);


            return typeBuilder.CreateTypeInfo().AsType();
        }

        private static class ConstructorBuilderHelpers
        {
            public static void DefineInterfaceProxyConstructor(Type interfaceType, TypeBuilder typeBuilder, FieldTable fieldTable)
            {
                var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, MethodInfoConstant.ObjectCtor.CallingConvention, new Type[] { typeof(IServiceProvider), interfaceType });

                constructorBuilder.DefineParameter(1, ParameterAttributes.None, FieldBuilderHelpers.ServiceProvider);
                constructorBuilder.DefineParameter(2, ParameterAttributes.None, FieldBuilderHelpers.Target);

                var ilGen = constructorBuilder.GetILGenerator();
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, MethodInfoConstant.ObjectCtor);

                ilGen.EmitThis();
                ilGen.EmitLoadArg(1);
                ilGen.Emit(OpCodes.Stfld, fieldTable[FieldBuilderHelpers.ServiceProvider]);

                ilGen.EmitThis();
                ilGen.EmitLoadArg(2);
                ilGen.Emit(OpCodes.Stfld, fieldTable[FieldBuilderHelpers.Target]);

                ilGen.Emit(OpCodes.Ret);
            }
        }

        private static class GenericParameterHelpers
        {
            internal static void DefineGenericParameter(Type implType, TypeBuilder typeBuilder)
            {
                var genericArguments = implType.GetTypeInfo().GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
                var genericArgumentsBuilders = typeBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
                for (var index = 0; index < genericArguments.Length; index++)
                {
                    genericArgumentsBuilders[index].SetGenericParameterAttributes(genericArguments[index].GenericParameterAttributes);
                    foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                    {
                        if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                        if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                    }
                }
            }
        }

        private static class FieldBuilderHelpers
        {
            public const string ServiceProvider = "__serviceProvider";
            public const string Target = "__targetInstance";

            public static FieldTable DefineInterfaceProxyField(Type implType, TypeBuilder typeBuilder)
            {
                var fieldTable = new FieldTable();
                fieldTable[ServiceProvider] = typeBuilder.DefineField(ServiceProvider, typeof(IServiceProvider), FieldAttributes.Private);
                fieldTable[Target] = typeBuilder.DefineField(Target, implType, FieldAttributes.Private);
                return fieldTable;
            }
        }

        private class FieldTable
        {
            private readonly Dictionary<string, FieldBuilder> _table = new Dictionary<string, FieldBuilder>();

            public FieldBuilder this[string fieldName]
            {
                get
                {
                    return _table[fieldName];
                }
                set
                {
                    _table[value.Name] = value;
                }
            }
        }
    }
}