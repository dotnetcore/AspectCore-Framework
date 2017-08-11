using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection.Emit;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Core.Internal
{
    internal class ProxyGeneratorHelpers
    {
        private const string ProxyNameSpace = "AspectCore.Core.ProxyBuilder";
        private static readonly ModuleBuilder _moduleBuilder;
        private static readonly Dictionary<string, Type> _definedTypes;

        static ProxyGeneratorHelpers()
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(ProxyNameSpace), AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = asmBuilder.DefineDynamicModule("core");
            _definedTypes = new Dictionary<string, Type>();
        }

        internal static Type CreateInterfaceProxy(Type interfaceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
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
                proxyType = CreateInterfaceProxyInternal(name, interfaceType, implType, additionalInterfaces, aspectValidator);
                _definedTypes.Add(name, proxyType);
                return proxyType;
            }

            string GetProxyTypeName()
            {
                return $"{ProxyNameSpace}.{implType.Name}Proxy^{interfaceType.Name}";
            }
        }

        private static Type CreateInterfaceProxyInternal(string name, Type interfaceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            var interfaces = new Type[] { interfaceType }.Concat(additionalInterfaces).Distinct().ToArray();

            var (typeBuilder, fieldTable) = TypeBuilderHelpers.DefineType(name, interfaceType, typeof(object), interfaces);

            //define constructor
            ConstructorBuilderHelpers.DefineInterfaceProxyConstructor(interfaceType, typeBuilder, fieldTable);

            //define methods
            MethodBuilderHelpers.DefineInterfaceProxyMethods(interfaceType, implType, additionalInterfaces, aspectValidator, typeBuilder, fieldTable);

            return typeBuilder.CreateTypeInfo().AsType();
        }

        private static class TypeBuilderHelpers
        {
            public static (TypeBuilder, FieldTable) DefineType(string name, Type serviceType, Type parentType, Type[] interfaces)
            {
                //define proxy type for interface service
                var typeBuilder = _moduleBuilder.DefineType(name, TypeAttributes.Class | TypeAttributes.Public, parentType, interfaces);

                //define genericParameter
                GenericParameterHelpers.DefineGenericParameter(serviceType, typeBuilder);

                //define default attribute
                typeBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(typeof(NonAspectAttribute)));
                typeBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(typeof(DynamicallyAttribute)));

                //define private field
                var fieldTable = FieldBuilderHelpers.DefineInterfaceProxyField(serviceType, typeBuilder);

                return (typeBuilder, fieldTable);
            }
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

        private static class MethodBuilderHelpers
        {
            const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            const MethodAttributes OverrideMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;

            public static void DefineInterfaceProxyMethods(Type interfaceType, Type targetType, Type[] additionalInterfaces,
                IAspectValidator aspectValidator, TypeBuilder typeBuilder, FieldTable fieldTable)
            {

            }

            private static MethodBuilder DefineMethod(MethodInfo method, MethodAttributes attributes, Type targetType, IAspectValidator aspectValidator,
                TypeBuilder typeBuilder, FieldTable fieldTable)
            {
                var methodBuilder = typeBuilder.DefineMethod(method.GetFullName(), attributes, method.CallingConvention, method.ReturnType, method.GetParameterTypes());

                GenericParameterHelpers.DefineGenericParameter(method, methodBuilder);

                //define method attributes
                methodBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(typeof(DynamicallyAttribute)));

                //inherit targetMethod's attribute
                foreach (var customAttributeData in method.CustomAttributes)
                {
                    methodBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(customAttributeData));
                }

                //define paramters
                ParameterBuilderHelpers.DefineParameters(method, methodBuilder);

                var implMethod = targetType.GetTypeInfo().GetMethod(new MethodSignature(method));

                if (aspectValidator.Validate(method))
                {
                    EmitProxyMethodBody();
                }
                else
                {
                    EmitMethodBody();
                }
                return methodBuilder;

                void EmitMethodBody()
                {
                    var ilGen = methodBuilder.GetILGenerator();
                    var parameters = method.GetParameterTypes();
                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Ldfld, fieldTable[FieldBuilderHelpers.Target]);
                    for (int i = 1; i <= parameters.Length; i++)
                    {
                        ilGen.EmitLoadArg(i);
                    }
                    ilGen.Emit(method.IsCallvirt() ? OpCodes.Callvirt : OpCodes.Call, method);
                    ilGen.Emit(OpCodes.Ret);
                }

                void EmitProxyMethodBody()
                {
                    var ilGen = methodBuilder.GetILGenerator();
                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Ldfld, fieldTable[FieldBuilderHelpers.ServiceProvider]);
                    ilGen.Emit(OpCodes.Call, MethodInfoConstant.GetAspectActivator);

                    EmitInitializeMetaData(ilGen);

                    ilGen.Emit(OpCodes.Ret);
                }

                void EmitInitializeMetaData(ILGenerator ilGen)
                {

                }
            }
        }

        private static class ParameterBuilderHelpers
        {
            public static void DefineParameters(MethodInfo targetMethod,MethodBuilder methodBuilder)
            {
                var parameters = targetMethod.GetParameters();
                if (parameters.Length > 0)
                {
                    var paramOffset = 1;   // 1
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        var parameterBuilder = methodBuilder.DefineParameter(i + paramOffset, parameter.Attributes, parameter.Name);
                        if (parameter.HasDefaultValue)
                        {
                            parameterBuilder.SetConstant(parameter.DefaultValue);
                        }
                        parameterBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(typeof(DynamicallyAttribute)));
                        foreach (var attribute in parameter.CustomAttributes)
                        {
                            parameterBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(attribute));
                        }
                    }
                }

                var returnParamter = targetMethod.ReturnParameter;
                var returnParameterBuilder = methodBuilder.DefineParameter(0, returnParamter.Attributes, returnParamter.Name);
                returnParameterBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(typeof(DynamicallyAttribute)));
                foreach (var attribute in returnParamter.CustomAttributes)
                {
                    returnParameterBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(attribute));
                }
            }
        }

        private static class GenericParameterHelpers
        {
            internal static void DefineGenericParameter(Type targetType, TypeBuilder typeBuilder)
            {
                if (!targetType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    return;
                }
                var genericArguments = targetType.GetTypeInfo().GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
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

            internal static void DefineGenericParameter(MethodInfo tergetMethod, MethodBuilder methodBuilder)
            {
                if (!tergetMethod.IsGenericMethod)
                {
                    return;
                }
                var genericArguments = tergetMethod.GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
                var genericArgumentsBuilders = methodBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
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

        private static class CustomAttributeBuilderHelpers
        {
            public static CustomAttributeBuilder DefineCustomAttribute(Type attributeType)
            {
                return new CustomAttributeBuilder(attributeType.GetTypeInfo().GetConstructor(Type.EmptyTypes), EmptyArray<object>.Value);
            }

            public static CustomAttributeBuilder DefineCustomAttribute(CustomAttributeData customAttributeData)
            {
                if (customAttributeData.NamedArguments != null)
                {
                    var attributeTypeInfo = customAttributeData.AttributeType.GetTypeInfo();
                    var constructor = customAttributeData.Constructor;
                    var constructorArgs = customAttributeData.ConstructorArguments.Select(c => c.Value).ToArray();
                    var namedProperties = customAttributeData.NamedArguments
                            .Where(n => !n.IsField)
                            .Select(n => attributeTypeInfo.GetProperty(n.MemberName))
                            .ToArray();
                    var propertyValues = customAttributeData.NamedArguments
                             .Where(n => !n.IsField)
                             .Select(n => n.TypedValue.Value)
                             .ToArray();
                    var namedFields = customAttributeData.NamedArguments.Where(n => n.IsField)
                             .Select(n => attributeTypeInfo.GetField(n.MemberName))
                             .ToArray();
                    var fieldValues = customAttributeData.NamedArguments.Where(n => n.IsField)
                             .Select(n => n.TypedValue.Value)
                             .ToArray();
                    return new CustomAttributeBuilder(customAttributeData.Constructor, constructorArgs
                       , namedProperties
                       , propertyValues, namedFields, fieldValues);
                }
                else
                {
                    return new CustomAttributeBuilder(customAttributeData.Constructor,
                        customAttributeData.ConstructorArguments.Select(c => c.Value).ToArray());
                }
            }
        }

        private static class FieldBuilderHelpers
        {
            public const string ServiceProvider = "__serviceProvider";
            public const string Target = "__targetInstance";

            public static FieldTable DefineInterfaceProxyField(Type targetType, TypeBuilder typeBuilder)
            {
                var fieldTable = new FieldTable();
                fieldTable[ServiceProvider] = typeBuilder.DefineField(ServiceProvider, typeof(IServiceProvider), FieldAttributes.Private);
                fieldTable[Target] = typeBuilder.DefineField(Target, targetType, FieldAttributes.Private);
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