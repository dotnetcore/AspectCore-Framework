using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using AspectCore.Extensions.Reflection.Emit;

namespace AspectCore.Utils
{
    internal class ProxyGeneratorUtils
    {
        private const string ProxyNameSpace = "AspectCore.DynamicGenerated";
        private const string ProxyAssemblyName = "AspectCore.DynamicProxy.Generator";
        private static readonly ModuleBuilder _moduleBuilder;
        private static readonly Dictionary<string, Type> _definedTypes;
        private static readonly object _lock = new object();

        static ProxyGeneratorUtils()
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(ProxyAssemblyName), AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = asmBuilder.DefineDynamicModule("core");
            _definedTypes = new Dictionary<string, Type>();
        }

        internal static Type CreateInterfaceProxy(Type interfaceType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            if (!interfaceType.GetTypeInfo().IsAccessibility())
            {
                throw new InvalidOperationException($"Validate '{interfaceType}' failed because the type does not satisfy the conditions of the generate proxy class.");
            }

            lock (_lock)
            {
                var name = ProxyNameUtils.GetInterfaceImplTypeFullName(interfaceType);
                if (!_definedTypes.TryGetValue(name, out Type type))
                {
                    type = CreateInterfaceImplInternal(name, interfaceType, additionalInterfaces, aspectValidator);
                    _definedTypes[name] = type;
                }
                return type;
            }
        }

        internal static Type CreateInterfaceProxy(Type interfaceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            if (!interfaceType.GetTypeInfo().IsAccessibility())
            {
                throw new InvalidOperationException($"Validate '{interfaceType}' failed because the type does not satisfy the conditions of the generate proxy class.");
            }

            lock (_lock)
            {
                var name = ProxyNameUtils.GetProxyTypeName(interfaceType, implType);
                if (!_definedTypes.TryGetValue(name, out Type type))
                {
                    type = CreateInterfaceProxyInternal(name, interfaceType, implType, additionalInterfaces, aspectValidator);
                    _definedTypes[name] = type;
                }
                return type;
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

            lock (_lock)
            {
                var name = ProxyNameUtils.GetProxyTypeName(serviceType, implType);
                if (!_definedTypes.TryGetValue(name, out Type type))
                {
                    type = CreateClassProxyInternal(name, serviceType, implType, additionalInterfaces, aspectValidator);
                    _definedTypes[name] = type;
                }
                return type;
            }
        }

        private static Type CreateInterfaceImplInternal(string name, Type interfaceType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            var interfaceTypes = new Type[] { interfaceType }.Concat(additionalInterfaces).Distinct().ToArray();
            var implTypeBuilder = _moduleBuilder.DefineType(name, TypeAttributes.Public, typeof(object), interfaceTypes);

            GenericParameterUtils.DefineGenericParameter(interfaceType, implTypeBuilder);

            ConstructorBuilderUtils.DefineInterfaceImplConstructor(implTypeBuilder);

            MethodBuilderUtils.DefineInterfaceImplMethods(interfaceTypes, implTypeBuilder);

            PropertyBuilderUtils.DefineInterfaceImplProperties(interfaceTypes, implTypeBuilder);

            var implType = implTypeBuilder.CreateTypeInfo().AsType();

            var typeDesc = TypeBuilderUtils.DefineType(ProxyNameUtils.GetProxyTypeName(ProxyNameUtils.GetInterfaceImplTypeName(interfaceType), interfaceType, implType),
                interfaceType, typeof(object), interfaceTypes);

            typeDesc.Properties[typeof(IAspectValidator).Name] = aspectValidator;

            //define constructor
            ConstructorBuilderUtils.DefineInterfaceProxyConstructor(interfaceType, implType, typeDesc);
            //define methods
            MethodBuilderUtils.DefineInterfaceProxyMethods(interfaceType, implType, additionalInterfaces, typeDesc);

            PropertyBuilderUtils.DefineInterfaceProxyProperties(interfaceType, implType, additionalInterfaces, typeDesc);

            return typeDesc.Compile();
        }

        private static Type CreateInterfaceProxyInternal(string name, Type interfaceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            var interfaces = new Type[] { interfaceType }.Concat(additionalInterfaces).Distinct().ToArray();

            var typeDesc = TypeBuilderUtils.DefineType(name, interfaceType, typeof(object), interfaces);

            typeDesc.Properties[typeof(IAspectValidator).Name] = aspectValidator;

            //define constructor
            ConstructorBuilderUtils.DefineInterfaceProxyConstructor(interfaceType, typeDesc);

            //define methods
            MethodBuilderUtils.DefineInterfaceProxyMethods(interfaceType, implType, additionalInterfaces, typeDesc);

            PropertyBuilderUtils.DefineInterfaceProxyProperties(interfaceType, implType, additionalInterfaces, typeDesc);

            return typeDesc.Compile();
        }

        private static Type CreateClassProxyInternal(string name, Type serviceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            var interfaces = additionalInterfaces.Distinct().ToArray();

            var typeDesc = TypeBuilderUtils.DefineType(name, serviceType, implType, interfaces);

            typeDesc.Properties[typeof(IAspectValidator).Name] = aspectValidator;

            //define constructor
            ConstructorBuilderUtils.DefineClassProxyConstructors(serviceType, implType, typeDesc);

            //define methods
            MethodBuilderUtils.DefineClassProxyMethods(serviceType, implType, additionalInterfaces, typeDesc);

            PropertyBuilderUtils.DefineClassProxyProperties(serviceType, implType, additionalInterfaces, typeDesc);

            return typeDesc.Compile();
        }

        private class ProxyNameUtils
        {
            private static readonly Dictionary<string, ProxyNameIndex> _indexs = new Dictionary<string, ProxyNameIndex>();
            private static readonly Dictionary<Tuple<Type,Type>, string> _indexMaps = new Dictionary<Tuple<Type, Type>, string>();

            private static string GetProxyTypeIndex(string className, Type serviceType, Type implementationType)
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

            public static string GetInterfaceImplTypeName(Type interfaceType)
            {
                var className = interfaceType.GetName();
                if (className.StartsWith("I", StringComparison.Ordinal))
                {
                    className = className.Substring(1);
                }
                return className /*+ "Impl"*/;
            }

            public static string GetInterfaceImplTypeFullName(Type interfaceType)
            {
                var className = GetInterfaceImplTypeName(interfaceType);
                return $"{ProxyNameSpace}.{className}{GetProxyTypeIndex(className, interfaceType, interfaceType)}";
            }

            public static string GetProxyTypeName(Type serviceType, Type implType)
            {
                return $"{ProxyNameSpace}.{implType.GetName()}{GetProxyTypeIndex(implType.GetName(), serviceType, implType)}";
            }

            public static string GetProxyTypeName(string className, Type serviceType, Type implType)
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

        private class TypeBuilderUtils
        {
            public static TypeDesc DefineType(string name, Type serviceType, Type parentType, Type[] interfaces)
            {
                //define proxy type for interface service
                var typeBuilder = _moduleBuilder.DefineType(name, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, parentType, interfaces);

                //define genericParameter
                GenericParameterUtils.DefineGenericParameter(serviceType, typeBuilder);

                //define default attribute
                typeBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(typeof(NonAspectAttribute)));
                typeBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(typeof(DynamicallyAttribute)));

                //define private field
                var fieldTable = FieldBuilderUtils.DefineFields(serviceType, typeBuilder);

                return new TypeDesc(typeBuilder, fieldTable, new MethodConstantTable(typeBuilder));
            }
        }

        private class ConstructorBuilderUtils
        {
            internal static void DefineInterfaceImplConstructor(TypeBuilder typeBuilder)
            {
                var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, MethodUtils.ObjectCtor.CallingConvention, Type.EmptyTypes);
                var ilGen = constructorBuilder.GetILGenerator();
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, MethodUtils.ObjectCtor);
                ilGen.Emit(OpCodes.Ret);
            }

            internal static void DefineInterfaceProxyConstructor(Type interfaceType, Type implType, TypeDesc typeDesc)
            {
                var constructorBuilder = typeDesc.Builder.DefineConstructor(MethodAttributes.Public, MethodUtils.ObjectCtor.CallingConvention, new Type[] { typeof(IAspectActivatorFactory) });

                constructorBuilder.DefineParameter(1, ParameterAttributes.None, FieldBuilderUtils.ActivatorFactory);

                var ilGen = constructorBuilder.GetILGenerator();
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, MethodUtils.ObjectCtor);

                ilGen.EmitThis();
                ilGen.EmitLoadArg(1);
                ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderUtils.ActivatorFactory]);

                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Newobj, implType.GetTypeInfo().GetConstructor(Type.EmptyTypes));
                ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderUtils.Target]);

                ilGen.Emit(OpCodes.Ret);
            }

            internal static void DefineInterfaceProxyConstructor(Type interfaceType, TypeDesc typeDesc)
            {
                var constructorBuilder = typeDesc.Builder.DefineConstructor(MethodAttributes.Public, MethodUtils.ObjectCtor.CallingConvention, new Type[] { typeof(IAspectActivatorFactory), interfaceType });

                constructorBuilder.DefineParameter(1, ParameterAttributes.None, FieldBuilderUtils.ActivatorFactory);
                constructorBuilder.DefineParameter(2, ParameterAttributes.None, FieldBuilderUtils.Target);

                var ilGen = constructorBuilder.GetILGenerator();
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, MethodUtils.ObjectCtor);

                ilGen.EmitThis();
                ilGen.EmitLoadArg(1);
                ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderUtils.ActivatorFactory]);

                ilGen.EmitThis();
                ilGen.EmitLoadArg(2);
                ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderUtils.Target]);

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
                    var parameters = new Type[] { typeof(IAspectActivatorFactory) }.Concat(parameterTypes).ToArray();
                    var constructorBuilder = typeDesc.Builder.DefineConstructor(constructor.Attributes, constructor.CallingConvention, parameters);

                    constructorBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(typeof(DynamicallyAttribute)));

                    //inherit constructor's attribute
                    foreach (var customAttributeData in constructor.CustomAttributes)
                    {
                        constructorBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(customAttributeData));
                    }

                    ParameterBuilderUtils.DefineParameters(constructor, constructorBuilder);

                    var ilGen = constructorBuilder.GetILGenerator();
                    ilGen.EmitThis();
                    for (var i = 2; i <= parameters.Length; i++)
                    {
                        ilGen.EmitLoadArg(i);
                    }
                    ilGen.Emit(OpCodes.Call, constructor);

                    ilGen.EmitThis();
                    ilGen.EmitLoadArg(1);
                    ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderUtils.ActivatorFactory]);

                    ilGen.EmitThis();
                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderUtils.Target]);

                    ilGen.Emit(OpCodes.Ret);
                }
            }
        }

        private class MethodBuilderUtils
        {
            const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            internal const MethodAttributes InterfaceMethodAttributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            const MethodAttributes OverrideMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;
            private static readonly HashSet<string> ignores = new HashSet<string> { "Finalize" };

            internal static void DefineInterfaceImplMethods(Type[] interfaceTypes, TypeBuilder implTypeBuilder)
            {
                foreach (var item in interfaceTypes)
                {
                    foreach (var method in item.GetTypeInfo().DeclaredMethods.Where(x => !x.IsPropertyBinding()))
                    {
                        DefineInterfaceImplMethod(method, implTypeBuilder);
                    }
                }
            }

            internal static MethodBuilder DefineInterfaceImplMethod(MethodInfo method, TypeBuilder implTypeBuilder)
            {
                var methodBuilder = implTypeBuilder.DefineMethod(method.Name, InterfaceMethodAttributes, method.CallingConvention, method.ReturnType, method.GetParameterTypes());
                var ilGen = methodBuilder.GetILGenerator();
                if (method.ReturnType != typeof(void))
                {
                    ilGen.EmitDefault(method.ReturnType);
                }
                ilGen.Emit(OpCodes.Ret);
                implTypeBuilder.DefineMethodOverride(methodBuilder, method);
                return methodBuilder;
            }

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
                foreach (var method in serviceType.GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => !x.IsPropertyBinding()))
                {
                    if (method.IsAccessibility() && !ignores.Contains(method.Name))
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

                GenericParameterUtils.DefineGenericParameter(method, methodBuilder);

                //define method attributes
                methodBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(typeof(DynamicallyAttribute)));

                //inherit targetMethod's attribute
                foreach (var customAttributeData in method.CustomAttributes)
                {
                    methodBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(customAttributeData));
                }

                //define paramters
                ParameterBuilderUtils.DefineParameters(method, methodBuilder);

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
                    ilGen.Emit(OpCodes.Ldfld, typeDesc.Fields[FieldBuilderUtils.Target]);
                    for (int i = 1; i <= parameters.Length; i++)
                    {
                        ilGen.EmitLoadArg(i);
                    }

                    var implMethod = implType.GetTypeInfo().GetMethod(new MethodSignature(method)) ?? implType.GetTypeInfo().GetExplicitMethod(method);    
                    if (implMethod == null)
                    {
                        throw new MissingMethodException($"Type '{implType}' does not contain a method '{method}'.");
                    }

                    ilGen.Emit(implMethod.IsCallvirt() ? OpCodes.Callvirt : OpCodes.Call, implMethod);
                    ilGen.Emit(OpCodes.Ret);
                }

                void EmitProxyMethodBody()
                {
                    var ilGen = methodBuilder.GetILGenerator();
                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Ldfld, typeDesc.Fields[FieldBuilderUtils.ActivatorFactory]);
                    ilGen.Emit(OpCodes.Callvirt, MethodUtils.CreateAspectActivator);
                    EmitInitializeMetaData(ilGen);
                    EmitReturnVaule(ilGen);
                    ilGen.Emit(OpCodes.Ret);
                }

                void EmitInitializeMetaData(ILGenerator ilGen)
                {
                    var serviceMethod = method;

                    var implMethod = implType.GetTypeInfo().GetMethod(new MethodSignature(serviceMethod)) ?? implType.GetTypeInfo().GetExplicitMethod(method); ;

                    if (implMethod == null)
                    {
                        throw new MissingMethodException($"Type '{implType}' does not contain a method '{method}'.");
                    }

                    var methodConstants = typeDesc.MethodConstants;

                    if (method.IsGenericMethodDefinition)
                    {
                        ilGen.EmitMethod(serviceMethod.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                        ilGen.EmitMethod(implMethod.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                        ilGen.EmitMethod(methodBuilder.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                    }
                    else
                    {
                        //methodConstants.AddMethod($"service{serviceMethod.GetFullName()}", serviceMethod);
                        //methodConstants.AddMethod($"imp{implMethod.GetFullName()}", implMethod);
                        //methodConstants.AddMethod($"proxy{methodBuilder.GetFullName()}", methodBuilder);

                        //methodConstants.LoadMethod(ilGen, $"service{serviceMethod.GetFullName()}");
                        //methodConstants.LoadMethod(ilGen, $"imp{implMethod.GetFullName()}");
                        //methodConstants.LoadMethod(ilGen, $"proxy{methodBuilder.GetFullName()}");
                        ilGen.EmitMethod(serviceMethod);
                        ilGen.EmitMethod(implMethod);
                        ilGen.EmitMethod(methodBuilder);
                    }

                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Ldfld, typeDesc.Fields[FieldBuilderUtils.Target]);
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
                    ilGen.Emit(OpCodes.Newobj, MethodUtils.AspectActivatorContexCtor);

                    if (method.ReturnType == typeof(void))
                    {
                        ilGen.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvoke.MakeGenericMethod(typeof(object)));
                        ilGen.Emit(OpCodes.Pop);
                    }
                    else if (method.ReturnType == typeof(Task))
                    {
                        ilGen.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvokeTask.MakeGenericMethod(typeof(object)));
                    }
                    else if (method.IsReturnTask())
                    {
                        var returnType = method.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                        ilGen.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvokeTask.MakeGenericMethod(returnType));
                    }
                    else if (method.IsReturnValueTask())
                    {
                        var returnType = method.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                        ilGen.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvokeValueTask.MakeGenericMethod(returnType));
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvoke.MakeGenericMethod(method.ReturnType));
                    }
                }
            }
        }

        private class PropertyBuilderUtils
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
                foreach (var property in serviceType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
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
                    var method = MethodBuilderUtils.DefineClassMethod(property.GetMethod, implType, typeDesc);
                    propertyBuilder.SetGetMethod(method);
                }
                if (property.CanWrite)
                {
                    var method = MethodBuilderUtils.DefineClassMethod(property.SetMethod, implType, typeDesc);
                    propertyBuilder.SetSetMethod(method);
                }
            }

            private static void DefineInterfacePropertyMethod(PropertyBuilder propertyBuilder, PropertyInfo property, Type implType, TypeDesc typeDesc)
            {
                if (property.CanRead)
                {
                    var method = MethodBuilderUtils.DefineInterfaceMethod(property.GetMethod, implType, typeDesc);
                    propertyBuilder.SetGetMethod(method);
                }
                if (property.CanWrite)
                {
                    var method = MethodBuilderUtils.DefineInterfaceMethod(property.SetMethod, implType, typeDesc);
                    propertyBuilder.SetSetMethod(method);
                }
            }

            private static void DefineExplicitPropertyMethod(PropertyBuilder propertyBuilder, PropertyInfo property, Type implType, TypeDesc typeDesc)
            {
                if (property.CanRead)
                {
                    var method = MethodBuilderUtils.DefineExplicitMethod(property.GetMethod, implType, typeDesc);
                    propertyBuilder.SetGetMethod(method);
                }
                if (property.CanWrite)
                {
                    var method = MethodBuilderUtils.DefineExplicitMethod(property.SetMethod, implType, typeDesc);
                    propertyBuilder.SetSetMethod(method);
                }
            }

            private static PropertyBuilder DefineInterfaceProxyProperty(PropertyInfo property, string name, Type implType, TypeDesc typeDesc)
            {
                var propertyBuilder = typeDesc.Builder.DefineProperty(name, property.Attributes, property.PropertyType, Type.EmptyTypes);

                propertyBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(typeof(DynamicallyAttribute)));

                //inherit targetMethod's attribute
                foreach (var customAttributeData in property.CustomAttributes)
                {
                    propertyBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(customAttributeData));
                }

                return propertyBuilder;
            }

            internal static void DefineInterfaceImplProperties(Type[] interfaceTypes, TypeBuilder implTypeBuilder)
            {
                foreach (var item in interfaceTypes)
                {
                    foreach (var property in item.GetTypeInfo().DeclaredProperties)
                    {
                        DefineInterfaceImplProperty(property, implTypeBuilder);
                    }
                }
            }

            private static void DefineInterfaceImplProperty(PropertyInfo property, TypeBuilder implTypeBuilder)
            {
                var propertyBuilder = implTypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, Type.EmptyTypes);
                var field = implTypeBuilder.DefineField($"<{property.Name}>k__BackingField", property.PropertyType, FieldAttributes.Private);
                if (property.CanRead)
                {
                    var methodBuilder = implTypeBuilder.DefineMethod(property.GetMethod.Name, MethodBuilderUtils.InterfaceMethodAttributes, property.GetMethod.CallingConvention, property.GetMethod.ReturnType, property.GetMethod.GetParameterTypes());
                    var ilGen = methodBuilder.GetILGenerator();
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldfld, field);
                    ilGen.Emit(OpCodes.Ret);
                    implTypeBuilder.DefineMethodOverride(methodBuilder, property.GetMethod);
                    propertyBuilder.SetGetMethod(methodBuilder);
                }
                if (property.CanWrite)
                {
                    var methodBuilder = implTypeBuilder.DefineMethod(property.SetMethod.Name, MethodBuilderUtils.InterfaceMethodAttributes, property.SetMethod.CallingConvention, property.SetMethod.ReturnType, property.SetMethod.GetParameterTypes());
                    var ilGen = methodBuilder.GetILGenerator();
                    ilGen.Emit(OpCodes.Ldarg_0);
                    ilGen.Emit(OpCodes.Ldarg_1);
                    ilGen.Emit(OpCodes.Stfld, field);
                    ilGen.Emit(OpCodes.Ret);
                    implTypeBuilder.DefineMethodOverride(methodBuilder, property.SetMethod);
                    propertyBuilder.SetSetMethod(methodBuilder);
                }

                foreach (var customAttributeData in property.CustomAttributes)
                {
                    propertyBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(customAttributeData));
                }
            }
        }

        private class ParameterBuilderUtils
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
                            if (!(parameter.ParameterType.GetTypeInfo().IsValueType && parameter.DefaultValue == null))
                                parameterBuilder.SetConstant(parameter.DefaultValue);
                        }
                        parameterBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(typeof(DynamicallyAttribute)));
                        foreach (var attribute in parameter.CustomAttributes)
                        {
                            parameterBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(attribute));
                        }
                    }
                }

                var returnParamter = targetMethod.ReturnParameter;
                var returnParameterBuilder = methodBuilder.DefineParameter(0, returnParamter.Attributes, returnParamter.Name);
                returnParameterBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(typeof(DynamicallyAttribute)));
                foreach (var attribute in returnParamter.CustomAttributes)
                {
                    returnParameterBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(attribute));
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
                        parameterBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(typeof(DynamicallyAttribute)));
                        foreach (var attribute in parameter.CustomAttributes)
                        {
                            parameterBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(attribute));
                        }
                    }
                }
            }
        }

        private class GenericParameterUtils
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
                    genericArgumentsBuilders[index].SetGenericParameterAttributes(ToClassGenericParameterAttributes(genericArguments[index].GenericParameterAttributes));
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

            private static GenericParameterAttributes ToClassGenericParameterAttributes(GenericParameterAttributes attributes)
            {
                if (attributes == GenericParameterAttributes.None)
                {
                    return GenericParameterAttributes.None;
                }
                if (attributes.HasFlag(GenericParameterAttributes.SpecialConstraintMask))
                {
                    return GenericParameterAttributes.SpecialConstraintMask;
                }
                if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                {
                    return GenericParameterAttributes.NotNullableValueTypeConstraint;
                }
                if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                {
                    return GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
                }
                if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                {
                    return GenericParameterAttributes.ReferenceTypeConstraint;
                }
                if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                {
                    return GenericParameterAttributes.DefaultConstructorConstraint;
                }
                return GenericParameterAttributes.None;
            }
        }

        private class CustomAttributeBuildeUtils
        {
            public static CustomAttributeBuilder DefineCustomAttribute(Type attributeType)
            {
                return new CustomAttributeBuilder(attributeType.GetTypeInfo().GetConstructor(Type.EmptyTypes), ArrayUtils.Empty<object>());
            }

            public static CustomAttributeBuilder DefineCustomAttribute(CustomAttributeData customAttributeData)
            {
                if (customAttributeData.NamedArguments != null)
                {
                    var attributeTypeInfo = customAttributeData.AttributeType.GetTypeInfo();
                    var constructor = customAttributeData.Constructor;
                    //var constructorArgs = customAttributeData.ConstructorArguments.Select(c => c.Value).ToArray();
                    var constructorArgs = new object[customAttributeData.ConstructorArguments.Count];
                    for (var i = 0; i < constructorArgs.Length; i++)
                    {
                        if (customAttributeData.ConstructorArguments[i].ArgumentType.IsArray)
                        {
                            constructorArgs[i] = ((IEnumerable<CustomAttributeTypedArgument>)customAttributeData.ConstructorArguments[i].Value).
                        Select(x => x.Value).ToArray();
                        }
                        else
                        {
                            constructorArgs[i] = customAttributeData.ConstructorArguments[i].Value;
                        }
                       
                    }
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

        private class FieldBuilderUtils
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
                name = name.GetHashCode().ToString();
                if (!_fields.ContainsKey(name))
                {
                    var field = _nestedTypeBuilder.DefineField(name, typeof(MethodInfo), FieldAttributes.Static | FieldAttributes.InitOnly | FieldAttributes.Assembly);
                    _fields.Add(name, field);
                    if (method != null)
                    {
                        _ilGen.EmitMethod(method);
                        _ilGen.Emit(OpCodes.Stsfld, field);
                    }
                }
            }

            public void LoadMethod(ILGenerator ilGen, string name)
            {
                name = name.GetHashCode().ToString();
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