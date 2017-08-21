using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Core.Internal
{
    internal class ProxyGeneratorHelpers
    {
        private const string ProxyNameSpace = "AspectCore.ProxyBuilder";
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
            if (!interfaceType.GetTypeInfo().IsAccessibility())
            {
                throw new InvalidOperationException($"Validate '{interfaceType}' failed because the type does not satisfy the conditions of the generate proxy class.");
            }

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

        internal static Type CreateClassProxy(Type serviceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            if (!serviceType.GetTypeInfo().IsAccessibility())
            {
                throw new InvalidOperationException($"Validate '{serviceType}' failed because the type does not satisfy the conditions of the generate proxy class.");
            }
            if (!implType.GetTypeInfo().CanInherited())
            {
                throw new InvalidOperationException($"Validate '{implType}' failed because the type does not satisfy the condition to be inherited.");
            }
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
                proxyType = CreateClassProxyInternal(name, serviceType, implType, additionalInterfaces, aspectValidator);
                _definedTypes.Add(name, proxyType);
                return proxyType;
            }

            string GetProxyTypeName()
            {
                return $"{ProxyNameSpace}.{implType.Name}Proxy";
            }
        }

        private static Type CreateInterfaceProxyInternal(string name, Type interfaceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            var interfaces = new Type[] { interfaceType }.Concat(additionalInterfaces).Distinct().ToArray();

            var typeDesc = TypeBuilderHelpers.DefineType(name, interfaceType, typeof(object), interfaces);

            typeDesc.Properties[typeof(IAspectValidator).Name] = aspectValidator;

            //define constructor
            ConstructorBuilderHelpers.DefineInterfaceProxyConstructor(interfaceType, typeDesc);

            //define methods
            MethodBuilderHelpers.DefineInterfaceProxyMethods(interfaceType, implType, additionalInterfaces, typeDesc);

            PropertyBuilderHelpers.DefineInterfaceProxyProperties(interfaceType, implType, additionalInterfaces, typeDesc);

            return typeDesc.Compile();
        }

        private static Type CreateClassProxyInternal(string name, Type serviceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            var interfaces = additionalInterfaces.Distinct().ToArray();

            var typeDesc = TypeBuilderHelpers.DefineType(name, serviceType, implType, interfaces);

            typeDesc.Properties[typeof(IAspectValidator).Name] = aspectValidator;

            //define constructor
            ConstructorBuilderHelpers.DefineClassProxyConstructors(serviceType, implType, typeDesc);

            //define methods
            MethodBuilderHelpers.DefineClassProxyMethods(serviceType, implType, additionalInterfaces, typeDesc);

            PropertyBuilderHelpers.DefineClassProxyProperties(serviceType, implType, additionalInterfaces, typeDesc);

            return typeDesc.Compile();
        }

        private class TypeBuilderHelpers
        {
            public static TypeDesc DefineType(string name, Type serviceType, Type parentType, Type[] interfaces)
            {
                //define proxy type for interface service
                var typeBuilder = _moduleBuilder.DefineType(name, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, parentType, interfaces);

                //define genericParameter
                GenericParameterHelpers.DefineGenericParameter(serviceType, typeBuilder);

                //define default attribute
                typeBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(typeof(NonAspectAttribute)));
                typeBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(typeof(DynamicallyAttribute)));

                //define private field
                var fieldTable = FieldBuilderHelpers.DefineFields(serviceType, typeBuilder);

                return new TypeDesc(typeBuilder, fieldTable, new MethodConstantTable(typeBuilder));
            }
        }

        private class ConstructorBuilderHelpers
        {
            internal static void DefineInterfaceProxyConstructor(Type interfaceType, TypeDesc typeDesc)
            {
                var constructorBuilder = typeDesc.Builder.DefineConstructor(MethodAttributes.Public, MethodInfoConstant.ObjectCtor.CallingConvention, new Type[] { typeof(IAspectActivatorFactory), interfaceType });

                constructorBuilder.DefineParameter(1, ParameterAttributes.None, FieldBuilderHelpers.ActivatorFactory);
                constructorBuilder.DefineParameter(2, ParameterAttributes.None, FieldBuilderHelpers.Target);

                var ilGen = constructorBuilder.GetILGenerator();
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, MethodInfoConstant.ObjectCtor);

                ilGen.EmitThis();
                ilGen.EmitLoadArg(1);
                ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderHelpers.ActivatorFactory]);

                ilGen.EmitThis();
                ilGen.EmitLoadArg(2);
                ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderHelpers.Target]);

                ilGen.Emit(OpCodes.Ret);
            }

            internal static void DefineClassProxyConstructors(Type serviceType, Type implType, TypeDesc typeDesc)
            {

                var constructors = implType.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic && (c.IsPublic || c.IsFamily || c.IsFamilyAndAssembly || c.IsFamilyOrAssembly)).ToArray();
                if (constructors.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"A suitable constructor for type {serviceType.FullName} could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.");
                }
                foreach (var constructor in constructors)
                {
                    var parameterTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
                    var parameters = new Type[] { typeof(IServiceProvider) }.Concat(parameterTypes).ToArray();
                    var constructorBuilder = typeDesc.Builder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, new Type[] { typeof(IAspectActivatorFactory) });

                    constructorBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(typeof(DynamicallyAttribute)));

                    //inherit constructor's attribute
                    foreach (var customAttributeData in constructor.CustomAttributes)
                    {
                        constructorBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(customAttributeData));
                    }

                    ParameterBuilderHelpers.DefineParameters(constructor, constructorBuilder);

                    var ilGen = constructorBuilder.GetILGenerator();
                    ilGen.EmitThis();
                    for (var i = 2; i <= parameters.Length; i++)
                    {
                        ilGen.EmitLoadArg(i);
                    }
                    ilGen.Emit(OpCodes.Call, constructor);

                    ilGen.EmitThis();
                    ilGen.EmitLoadArg(1);
                    ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderHelpers.ActivatorFactory]);

                    ilGen.EmitThis();
                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderHelpers.Target]);

                    ilGen.Emit(OpCodes.Ret);
                }
            }
        }

        private class MethodBuilderHelpers
        {
            const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            const MethodAttributes InterfaceMethodAttributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            const MethodAttributes OverrideMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;

            internal static void DefineInterfaceProxyMethods(Type interfaceType, Type targetType, Type[] additionalInterfaces, TypeDesc typeDesc)
            {     
                foreach (var method in interfaceType.GetTypeInfo().DeclaredMethods.Where(x => !x.IsPropertyBinding()))
                {
                    DefineInterfaceMethod(method, targetType, typeDesc);
                }
                foreach (var item in additionalInterfaces)
                {
                    foreach (var method in item.GetTypeInfo().DeclaredMethods.Where(x => !x.IsPropertyBinding()))
                    {
                        DefineExplicitMethod(method, targetType, typeDesc);
                    }
                }
            }

            internal static void DefineClassProxyMethods(Type serviceType, Type implType, Type[] additionalInterfaces, TypeDesc typeDesc)
            {
                foreach (var method in serviceType.GetTypeInfo().DeclaredMethods.Where(x => !x.IsPropertyBinding()))
                {
                    if (method.IsAccessibility())
                        DefineClassMethod(method, implType, typeDesc);
                }
                foreach (var item in additionalInterfaces)
                {
                    foreach (var method in item.GetTypeInfo().DeclaredMethods.Where(x => !x.IsPropertyBinding()))
                    {
                        DefineExplicitMethod(method, implType, typeDesc);
                    }
                }
            }

            internal static MethodBuilder DefineInterfaceMethod(MethodInfo method, Type implType, TypeDesc typeDesc)
            {
                var methodBuilder = DefineMethod(method, method.Name, InterfaceMethodAttributes, implType, typeDesc);
                typeDesc.Builder.DefineMethodOverride(methodBuilder, method);
                return methodBuilder;
            }

            internal static MethodBuilder DefineExplicitMethod(MethodInfo method, Type implType, TypeDesc typeDesc)
            {
                var methodBuilder = DefineMethod(method, method.GetFullName(), ExplicitMethodAttributes, implType, typeDesc);
                typeDesc.Builder.DefineMethodOverride(methodBuilder, method);
                return methodBuilder;
            }

            internal static MethodBuilder DefineClassMethod(MethodInfo method, Type implType, TypeDesc typeDesc)
            {
                var attributes = OverrideMethodAttributes;

                if (method.Attributes.HasFlag(MethodAttributes.Public))
                {
                    attributes = attributes | MethodAttributes.Public;
                }

                if (method.Attributes.HasFlag(MethodAttributes.Family))
                {
                    attributes = attributes | MethodAttributes.Family;
                }

                if (method.Attributes.HasFlag(MethodAttributes.FamORAssem))
                {
                    attributes = attributes | MethodAttributes.FamORAssem;
                }

                var methodBuilder = DefineMethod(method, method.Name, attributes, implType, typeDesc);
                return methodBuilder;
            }

            private static MethodBuilder DefineMethod(MethodInfo method, string name, MethodAttributes attributes, Type implType, TypeDesc typeDesc)
            {
                var methodBuilder = typeDesc.Builder.DefineMethod(name, attributes, method.CallingConvention, method.ReturnType, method.GetParameterTypes());

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

                if (typeDesc.GetProperty<IAspectValidator>().Validate(method))
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
                    ilGen.Emit(OpCodes.Ldfld, typeDesc.Fields[FieldBuilderHelpers.Target]);
                    for (int i = 1; i <= parameters.Length; i++)
                    {
                        ilGen.EmitLoadArg(i);
                    }

                    var implTypeIfGenericTypeDefinition = implType.GetTypeInfo().IsGenericTypeDefinition ?
                       implType.GetTypeInfo().MakeGenericType(typeDesc.Builder.GetGenericArguments()) :
                       implType;

                    var implMethod = implTypeIfGenericTypeDefinition.GetTypeInfo().GetMethod(new MethodSignature(method));

                    ilGen.Emit(implMethod.IsCallvirt() ? OpCodes.Callvirt : OpCodes.Call, method);
                    ilGen.Emit(OpCodes.Ret);
                }

                void EmitProxyMethodBody()
                {
                    var ilGen = methodBuilder.GetILGenerator();
                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Ldfld, typeDesc.Fields[FieldBuilderHelpers.ActivatorFactory]);
                    ilGen.Emit(OpCodes.Callvirt, MethodInfoConstant.CreateAspectActivator);
                    EmitInitializeMetaData(ilGen);
                    EmitReturnVaule(ilGen);
                    ilGen.Emit(OpCodes.Ret);
                }

                void EmitInitializeMetaData(ILGenerator ilGen)
                {
                    var serviceMethod = method;
                    if (serviceMethod.DeclaringType.GetTypeInfo().IsGenericTypeDefinition)
                    {
                        var serviceTypeOfGeneric = serviceMethod.DeclaringType.GetTypeInfo().MakeGenericType(typeDesc.Builder.GetGenericArguments());
                        serviceMethod = serviceTypeOfGeneric.GetTypeInfo().GetMethod(new MethodSignature(serviceMethod));
                    }

                    var implTypeIfGenericTypeDefinition = implType.GetTypeInfo().IsGenericTypeDefinition ?
                        implType.GetTypeInfo().MakeGenericType(typeDesc.Builder.GetGenericArguments()) :
                        implType;

                    var implMethod = implTypeIfGenericTypeDefinition.GetTypeInfo().GetMethod(new MethodSignature(serviceMethod));

                    var methodConstants = typeDesc.MethodConstants;

                    if (method.IsGenericMethodDefinition)
                    {
                        methodConstants.AddMethod($"service{serviceMethod.Name}", serviceMethod.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                        methodConstants.AddMethod($"imp{implMethod.Name}", implMethod.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                        methodConstants.AddMethod($"proxy{methodBuilder.Name}", methodBuilder.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                    }
                    else
                    {
                        methodConstants.AddMethod($"service{serviceMethod.Name}", serviceMethod);
                        methodConstants.AddMethod($"imp{implMethod.Name}", implMethod);
                        methodConstants.AddMethod($"proxy{methodBuilder.Name}", methodBuilder);
                    }

                    methodConstants.LoadMethod(ilGen, $"service{serviceMethod.Name}");
                    methodConstants.LoadMethod(ilGen, $"imp{implMethod.Name}");
                    methodConstants.LoadMethod(ilGen, $"proxy{methodBuilder.Name}");

                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Ldfld, typeDesc.Fields[FieldBuilderHelpers.Target]);
                    ilGen.EmitThis();
                    var parameters = method.GetParameterTypes();
                    if (parameters.Length == 0)
                    {
                        ilGen.Emit(OpCodes.Ldnull);
                        return;
                    }
                    ilGen.EmitInt(parameters.Length);
                    ilGen.Emit(OpCodes.Newarr, typeof(object));
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        ilGen.Emit(OpCodes.Dup);
                        ilGen.EmitInt(i);
                        ilGen.EmitLoadArg(i + 1);
                        ilGen.EmitConvertToObject(parameters[i]);
                        ilGen.Emit(OpCodes.Stelem_Ref);
                    }
                }

                void EmitReturnVaule(ILGenerator ilGen)
                {
                    ilGen.Emit(OpCodes.Newobj, MethodInfoConstant.AspectActivatorContexCtor);

                    if (method.ReturnType == typeof(void))
                    {
                        ilGen.Emit(OpCodes.Callvirt, MethodInfoConstant.AspectActivatorInvoke.MakeGenericMethod(typeof(object)));
                        ilGen.Emit(OpCodes.Pop);
                    }
                    else if (method.ReturnType == typeof(Task))
                    {
                        ilGen.Emit(OpCodes.Callvirt, MethodInfoConstant.AspectActivatorInvokeTask.MakeGenericMethod(typeof(object)));
                    }
                    else if (method.IsReturnTask())
                    {
                        var returnType = method.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                        ilGen.Emit(OpCodes.Callvirt, MethodInfoConstant.AspectActivatorInvokeTask.MakeGenericMethod(returnType));
                    }
                    else if (method.IsReturnValueTask())
                    {
                        var returnType = method.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                        ilGen.Emit(OpCodes.Callvirt, MethodInfoConstant.AspectActivatorInvokeValueTask.MakeGenericMethod(returnType));
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Callvirt, MethodInfoConstant.AspectActivatorInvoke.MakeGenericMethod(method.ReturnType));
                    }
                }
            }   
        }

        private class PropertyBuilderHelpers
        {
            public static void DefineInterfaceProxyProperties(Type interfaceType, Type implType, Type[] additionalInterfaces, TypeDesc typeDesc)
            {
                foreach (var property in interfaceType.GetTypeInfo().DeclaredProperties)
                {
                    var builder = DefineInterfaceProxyProperty(property, property.Name, implType, typeDesc);
                    DefineInterfacePropertyMethod(builder, property, implType, typeDesc);
                }
                foreach (var item in additionalInterfaces)
                {
                    foreach (var property in item.GetTypeInfo().DeclaredProperties)
                    {
                        var builder = DefineInterfaceProxyProperty(property, property.GetFullName(), implType, typeDesc);
                        DefineExplicitPropertyMethod(builder, property, implType, typeDesc);
                    }
                }
            }

            internal static void DefineClassProxyProperties(Type serviceType, Type implType, Type[] additionalInterfaces, TypeDesc typeDesc)
            {
                foreach (var property in serviceType.GetTypeInfo().DeclaredProperties)
                {
                    if (property.IsAccessibility())
                    {
                        var builder = DefineInterfaceProxyProperty(property, property.Name, implType, typeDesc);
                        DefineClassPropertyMethod(builder, property, implType, typeDesc);
                    }
                }
                foreach (var item in additionalInterfaces)
                {
                    foreach (var property in item.GetTypeInfo().DeclaredProperties)
                    {
                        var builder = DefineInterfaceProxyProperty(property, property.GetFullName(), implType, typeDesc);
                        DefineExplicitPropertyMethod(builder, property, implType, typeDesc);
                    }
                }
            }

            private static void DefineClassPropertyMethod(PropertyBuilder propertyBuilder, PropertyInfo property, Type implType, TypeDesc typeDesc)
            {
                if (property.CanRead)
                {
                    var method = MethodBuilderHelpers.DefineClassMethod(property.GetMethod, implType, typeDesc);
                    propertyBuilder.SetGetMethod(method);
                }
                if (property.CanWrite)
                {
                    var method = MethodBuilderHelpers.DefineClassMethod(property.SetMethod, implType, typeDesc);
                    propertyBuilder.SetSetMethod(method);
                }
            }

            private static void DefineInterfacePropertyMethod(PropertyBuilder propertyBuilder, PropertyInfo property, Type implType, TypeDesc typeDesc)
            {
                if (property.CanRead)
                {
                    var method = MethodBuilderHelpers.DefineInterfaceMethod(property.GetMethod, implType, typeDesc);
                    propertyBuilder.SetGetMethod(method);
                }
                if (property.CanWrite)
                {
                    var method = MethodBuilderHelpers.DefineInterfaceMethod(property.SetMethod, implType, typeDesc);
                    propertyBuilder.SetSetMethod(method);
                }
            }

            private static void DefineExplicitPropertyMethod(PropertyBuilder propertyBuilder, PropertyInfo property, Type implType, TypeDesc typeDesc)
            {
                if (property.CanRead)
                {
                    var method = MethodBuilderHelpers.DefineExplicitMethod(property.GetMethod, implType, typeDesc);
                    propertyBuilder.SetGetMethod(method);
                }
                if (property.CanWrite)
                {
                    var method = MethodBuilderHelpers.DefineExplicitMethod(property.SetMethod, implType, typeDesc);
                    propertyBuilder.SetSetMethod(method);
                }
            }

            private static PropertyBuilder DefineInterfaceProxyProperty(PropertyInfo property, string name, Type implType, TypeDesc typeDesc)
            {
                var propertyBuilder = typeDesc.Builder.DefineProperty(name, property.Attributes, property.PropertyType, Type.EmptyTypes);

                propertyBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(typeof(DynamicallyAttribute)));

                //inherit targetMethod's attribute
                foreach (var customAttributeData in property.CustomAttributes)
                {
                    propertyBuilder.SetCustomAttribute(CustomAttributeBuilderHelpers.DefineCustomAttribute(customAttributeData));
                }

                return propertyBuilder;
            }        
        }

        private class ParameterBuilderHelpers
        {
            public static void DefineParameters(MethodInfo targetMethod, MethodBuilder methodBuilder)
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

            internal static void DefineParameters(ConstructorInfo constructor, ConstructorBuilder constructorBuilder)
            {
                constructorBuilder.DefineParameter(1, ParameterAttributes.None, "aspectContextFactory");
                var parameters = constructor.GetParameters();
                if (parameters.Length > 0)
                {
                    var paramOffset = 2;    //ParameterTypes.Length - parameters.Length + 1
                    for (var i = 0; i < parameters.Length; i++)
                    {
                        var parameter = parameters[i];
                        var parameterBuilder = constructorBuilder.DefineParameter(i + paramOffset, parameter.Attributes, parameter.Name);
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
            }
        }

        private class GenericParameterHelpers
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

        private class CustomAttributeBuilderHelpers
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

        private class FieldBuilderHelpers
        {
            public const string ActivatorFactory = "__activatorFactory";
            public const string Target = "__targetInstance";

            public static FieldTable DefineFields(Type targetType, TypeBuilder typeBuilder)
            {
                var fieldTable = new FieldTable();
                fieldTable[ActivatorFactory] = typeBuilder.DefineField(ActivatorFactory, typeof(IAspectActivatorFactory), FieldAttributes.Private);
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

        private class MethodConstantTable
        {
            private readonly TypeBuilder _nestedTypeBuilder;
            private readonly ConstructorBuilder _constructorBuilder;
            private readonly ILGenerator _ilGen;
            private readonly Dictionary<string, FieldBuilder> _fields;

            public MethodConstantTable(TypeBuilder typeBuilder)
            {
                _fields = new Dictionary<string, FieldBuilder>();
                _nestedTypeBuilder = typeBuilder.DefineNestedType("MethodConstant", TypeAttributes.NestedPrivate);
                _constructorBuilder = _nestedTypeBuilder.DefineTypeInitializer();
                _ilGen = _constructorBuilder.GetILGenerator();
            }

            public void AddMethod(string name, MethodInfo method)
            {
                var field = _nestedTypeBuilder.DefineField(name, typeof(MethodInfo), FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Assembly);
                _fields.Add(name, field);
                if (method != null)
                {
                    _ilGen.EmitMethod(method);
                    _ilGen.Emit(OpCodes.Stsfld, field);
                }
            }

            public void LoadMethod(ILGenerator ilGen, string name)
            {
                if (_fields.TryGetValue(name, out FieldBuilder field))
                {
                    ilGen.Emit(OpCodes.Ldsfld, field);
                    return;
                }
                throw new InvalidOperationException($"Failed to find the method associated with the specified key {name}.");
            }

            public void Compile()
            {
                _ilGen.Emit(OpCodes.Ret);
                _nestedTypeBuilder.CreateTypeInfo();
            }
        }

        private class TypeDesc
        {
            public TypeBuilder Builder { get; }

            public FieldTable Fields { get; }

            public MethodConstantTable MethodConstants { get; }

            public Dictionary<string, object> Properties { get; }

            public TypeDesc(TypeBuilder typeBuilder, FieldTable fields, MethodConstantTable methodConstants)
            {
                Builder = typeBuilder;
                Fields = fields;
                MethodConstants = methodConstants;
                Properties = new Dictionary<string, object>();
            }

            public Type Compile()
            {
                MethodConstants.Compile();
                return Builder.CreateTypeInfo().AsType();
            }

            public T GetProperty<T>()
            {
                return (T)Properties[typeof(T).Name];
            }
        }
    }
}