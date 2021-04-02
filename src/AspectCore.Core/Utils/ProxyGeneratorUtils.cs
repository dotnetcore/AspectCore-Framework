using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
    /// <summary>
    /// 代理生成器工具
    /// </summary>
    internal class ProxyGeneratorUtils
    {
        /// <summary>
        /// 代理名称空间名称
        /// </summary>
        private const string ProxyNameSpace = "AspectCore.DynamicGenerated";

        /// <summary>
        /// 代理程序集名称
        /// </summary>
        private const string ProxyAssemblyName = "AspectCore.DynamicProxy.Generator";

        private readonly ModuleBuilder _moduleBuilder;
        private readonly Dictionary<string, Type> _definedTypes;
        private readonly object _lock = new object();
        private readonly ProxyNameUtils _proxyNameUtils;

        /// <summary>
        /// 代理生成器工具
        /// </summary>
        public ProxyGeneratorUtils()
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(ProxyAssemblyName), AssemblyBuilderAccess.RunAndCollect);
            _moduleBuilder = asmBuilder.DefineDynamicModule("core");
            _definedTypes = new Dictionary<string, Type>();
            _proxyNameUtils = new ProxyNameUtils();
        }

        /// <summary>
        /// 以接口代理方式创建代理类型
        /// </summary>
        /// <param name="interfaceType">接口类型</param>
        /// <param name="additionalInterfaces">其他的接口</param>
        /// <param name="aspectValidator">执行检查管道以确定方法是否需要被代理的对象</param>
        /// <returns>代理类型</returns>
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
            var interfaceTypes = new Type[] { interfaceType }.Concat(additionalInterfaces).Distinct().ToArray();
            var implTypeBuilder = _moduleBuilder.DefineType(name, TypeAttributes.Public, typeof(object), interfaceTypes);

            GenericParameterUtils.DefineGenericParameter(interfaceType, implTypeBuilder);

            ConstructorBuilderUtils.DefineInterfaceImplConstructor(implTypeBuilder);

            MethodBuilderUtils.DefineInterfaceImplMethods(interfaceTypes, implTypeBuilder);

            PropertyBuilderUtils.DefineInterfaceImplProperties(interfaceTypes, implTypeBuilder);

            var implType = implTypeBuilder.CreateTypeInfo().AsType();

            var typeDesc = TypeBuilderUtils.DefineType(_moduleBuilder, _proxyNameUtils.GetProxyTypeName(_proxyNameUtils.GetInterfaceImplTypeName(interfaceType), interfaceType, implType),
                interfaceType, typeof(object), interfaceTypes);

            typeDesc.Properties[typeof(IAspectValidator).Name] = aspectValidator;

            //define constructor
            ConstructorBuilderUtils.DefineInterfaceProxyConstructor(interfaceType, implType, typeDesc);
            //define methods
            MethodBuilderUtils.DefineInterfaceProxyMethods(interfaceType, implType, additionalInterfaces, typeDesc);

            PropertyBuilderUtils.DefineInterfaceProxyProperties(interfaceType, implType, additionalInterfaces, typeDesc);

            return typeDesc.Compile();
        }

        private Type CreateInterfaceProxyInternal(string name, Type interfaceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            var interfaces = new Type[] { interfaceType }.Concat(additionalInterfaces).Distinct().ToArray();

            var typeDesc = TypeBuilderUtils.DefineType(_moduleBuilder, name, interfaceType, typeof(object), interfaces);

            typeDesc.Properties[typeof(IAspectValidator).Name] = aspectValidator;

            //define constructor
            ConstructorBuilderUtils.DefineInterfaceProxyConstructor(interfaceType, typeDesc);

            //define methods
            MethodBuilderUtils.DefineInterfaceProxyMethods(interfaceType, implType, additionalInterfaces, typeDesc);

            PropertyBuilderUtils.DefineInterfaceProxyProperties(interfaceType, implType, additionalInterfaces, typeDesc);

            return typeDesc.Compile();
        }

        private Type CreateClassProxyInternal(string name, Type serviceType, Type implType, Type[] additionalInterfaces, IAspectValidator aspectValidator)
        {
            var interfaces = additionalInterfaces.Distinct().ToArray();

            var typeDesc = TypeBuilderUtils.DefineType(_moduleBuilder, name, serviceType, implType, interfaces);

            typeDesc.Properties[typeof(IAspectValidator).Name] = aspectValidator;

            //define constructor
            ConstructorBuilderUtils.DefineClassProxyConstructors(serviceType, implType, typeDesc);

            //define methods
            MethodBuilderUtils.DefineClassProxyMethods(serviceType, implType, additionalInterfaces, typeDesc);

            PropertyBuilderUtils.DefineClassProxyProperties(serviceType, implType, additionalInterfaces, typeDesc);

            return typeDesc.Compile();
        }

        /// <summary>
        /// 代理类名称生成工具
        /// </summary>
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

            /// <summary>
            /// 获取接口的实现类型的名称
            /// </summary>
            /// <param name="interfaceType">接口类型</param>
            /// <returns>实现类型的名称</returns>
            public string GetInterfaceImplTypeName(Type interfaceType)
            {
                var className = interfaceType.GetReflector().DisplayName;
                if (className.StartsWith("I", StringComparison.Ordinal))
                {
                    className = className.Substring(1);
                }
                return className /*+ "Impl"*/;
            }

            /// <summary>
            /// 获取接口的实现类型的全限定名
            /// </summary>
            /// <param name="interfaceType">接口类型</param>
            /// <returns>获取接口的实现类型的全限定名</returns>
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

        /// <summary>
        /// 产生一个递增的代理类名称索引值
        /// </summary>
        private class ProxyNameIndex
        {
            private int _index = -1;

            public int GenIndex()
            {
                return Interlocked.Increment(ref _index);
            }
        }

        /// <summary>
        /// 类型工具
        /// </summary>
        private class TypeBuilderUtils
        {
            /// <summary>
            /// 定义一个类型
            /// </summary>
            /// <param name="moduleBuilder">模块构建器</param>
            /// <param name="name">类型名称</param>
            /// <param name="serviceType">服务类型</param>
            /// <param name="parentType">父类型</param>
            /// <param name="interfaces">实现的接口</param>
            /// <returns>类型描述对象</returns>
            public static TypeDesc DefineType(ModuleBuilder moduleBuilder, string name, Type serviceType, Type parentType, Type[] interfaces)
            {
                //define proxy type for interface service
                var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, parentType, interfaces);

                //define genericParameter
                GenericParameterUtils.DefineGenericParameter(serviceType, typeBuilder);

                //define default attribute
                typeBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(typeof(NonAspectAttribute)));
                typeBuilder.SetCustomAttribute(CustomAttributeBuildeUtils.DefineCustomAttribute(typeof(DynamicallyAttribute)));

                //define private field
                var fieldTable = FieldBuilderUtils.DefineFields(serviceType, typeBuilder);

                return new TypeDesc(serviceType, typeBuilder, fieldTable, new MethodConstantTable(typeBuilder));
            }
        }

        /// <summary>
        /// 构造器构建工具
        /// </summary>
        private class ConstructorBuilderUtils
        {
            /// <summary>
            /// 定义无参构造器
            /// </summary>
            /// <param name="typeBuilder">TypeBuilder</param>
            internal static void DefineInterfaceImplConstructor(TypeBuilder typeBuilder)
            {
                var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, MethodUtils.ObjectCtor.CallingConvention, Type.EmptyTypes);
                var ilGen = constructorBuilder.GetILGenerator();
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, MethodUtils.ObjectCtor);
                ilGen.Emit(OpCodes.Ret);
            }

            /// <summary>
            /// 定义一个参数类型为IAspectActivatorFactory的代理构造器
            /// </summary>
            /// <param name="interfaceType">接口类型(?方法中并未使用)</param>
            /// <param name="implType">目标类型</param>
            /// <param name="typeDesc">类型描述</param>
            internal static void DefineInterfaceProxyConstructor(Type interfaceType, Type implType, TypeDesc typeDesc)
            {
                //定义一个接受IAspectActivatorFactory类型的构造器
                var constructorBuilder = typeDesc.Builder.DefineConstructor(MethodAttributes.Public, MethodUtils.ObjectCtor.CallingConvention, new Type[] { typeof(IAspectActivatorFactory) });

                constructorBuilder.DefineParameter(1, ParameterAttributes.None, FieldBuilderUtils.ActivatorFactory);

                var ilGen = constructorBuilder.GetILGenerator();

                //调用基类Object的构造函数
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, MethodUtils.ObjectCtor);

                //赋值代理对象中类型为IAspectActivatorFactory的字段
                ilGen.EmitThis();
                ilGen.EmitLoadArg(1);
                ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderUtils.ActivatorFactory]);

                //赋值代理对象中的目标对象
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Newobj, implType.GetTypeInfo().GetConstructor(Type.EmptyTypes));
                ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderUtils.Target]);

                ilGen.Emit(OpCodes.Ret);
            }

            /// <summary>
            /// 定义两个参数的代理构造器
            /// </summary>
            /// <remarks>
            /// 构造器参数说明：1.IAspectActivatorFactory;2.目标对象
            /// </remarks>
            /// <param name="interfaceType">接口类型</param>
            /// <param name="typeDesc">类型描述</param>
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

            /// <summary>
            /// 定义带其他参数的代理构造器
            /// </summary>
            /// <remarks>
            /// 前两个构造器参数固定为：1.IAspectActivatorFactory;2.目标对象
            /// </remarks>
            /// <param name="serviceType">服务类型(? 方法中只用于了返回友好错误信息)</param>
            /// <param name="implType">目标类型</param>
            /// <param name="typeDesc">类型描述</param>
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
                    ilGen.EmitLoadArg(1);
                    ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderUtils.ActivatorFactory]);

                    ilGen.EmitThis();
                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Stfld, typeDesc.Fields[FieldBuilderUtils.Target]);

                    ilGen.EmitThis();
                    for (var i = 2; i <= parameters.Length; i++)
                    {
                        ilGen.EmitLoadArg(i);
                    }
                    ilGen.Emit(OpCodes.Call, constructor);

                    ilGen.Emit(OpCodes.Ret);
                }
            }
        }

        /// <summary>
        /// 方法构建工具
        /// </summary>
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
                    if (method.IsVisibleAndVirtual() && !ignores.Contains(method.Name))
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
                var methodBuilder = DefineMethod(method, method.GetName(), ExplicitMethodAttributes, implType, typeDesc);
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

                var implementationMethod = implType.GetTypeInfo().GetMethodBySignature(method);
                if (implementationMethod == null)
                {
                    var interfaces = implType.GetInterfaces();
                    if (interfaces == null || interfaces.Length <= 0)
                    {
                        throw new MissingMethodException($"Type '{implType}' does not contain a method '{method}'.");
                    }
                    var @interface = interfaces.Where(f => f.GetCustomAttribute(typeof(AbstractInterceptorAttribute)) != null).ToArray();
                    if (@interface.Length > 0)
                    {
                        foreach (var item in @interface)
                        {
                            implementationMethod = item.GetTypeInfo().GetMethodBySignature(method);
                            if (implementationMethod != null) break;
                        }
                    }
                    else
                    {
                        foreach (var item in interfaces)
                        {
                            implementationMethod = item.GetTypeInfo().GetMethodBySignature(method);
                            if (implementationMethod != null) break;
                        }
                    }
                    if (implementationMethod == null)
                    {
                        throw new MissingMethodException($"Type '{implType}' does not contain a method '{method}'.");
                    }
                }

                if (method.IsNonAspect())
                {
                    EmitMethodBody();
                }
                else if (typeDesc.GetProperty<IAspectValidator>().Validate(method, true) || typeDesc.GetProperty<IAspectValidator>().Validate(implementationMethod, false))
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
                    if (typeDesc.ServiceType.GetTypeInfo().IsInterface || !implementationMethod.IsExplicit())
                    {
                        ilGen.EmitThis();
                        ilGen.Emit(OpCodes.Ldfld, typeDesc.Fields[FieldBuilderUtils.Target]);
                        for (int i = 1; i <= parameters.Length; i++)
                        {
                            ilGen.EmitLoadArg(i);
                        }
                        var callOpCode = implementationMethod.IsCallvirt() ? OpCodes.Callvirt : OpCodes.Call;
                        ilGen.Emit(callOpCode, implementationMethod.IsExplicit() ? method : implementationMethod);
                    }
                    else
                    {
                        var reflectorLocal = ilGen.DeclareLocal(typeof(MethodReflector));
                        var argsLocal = ilGen.DeclareLocal(typeof(object[]));
                        var returnLocal = ilGen.DeclareLocal(typeof(object));
                        ilGen.EmitMethod(implementationMethod);
                        ilGen.Emit(OpCodes.Call, MethodUtils.GetMethodReflector);
                        ilGen.Emit(OpCodes.Stloc, reflectorLocal);
                        ilGen.EmitInt(parameters.Length);
                        ilGen.Emit(OpCodes.Newarr, typeof(object));
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            ilGen.Emit(OpCodes.Dup);
                            ilGen.EmitInt(i);
                            ilGen.EmitLoadArg(i + 1);
                            if (parameters[i].IsByRef)
                            {
                                ilGen.EmitLdRef(parameters[i]);
                                ilGen.EmitConvertToObject(parameters[i].GetElementType());
                            }
                            else
                            {
                                ilGen.EmitConvertToObject(parameters[i]);
                            }
                            ilGen.Emit(OpCodes.Stelem_Ref);
                        }
                        ilGen.Emit(OpCodes.Stloc, argsLocal);
                        ilGen.Emit(OpCodes.Ldloc, reflectorLocal);
                        ilGen.EmitThis();
                        ilGen.Emit(OpCodes.Ldloc, argsLocal);
                        ilGen.Emit(OpCodes.Callvirt, MethodUtils.ReflectorInvoke);
                        ilGen.Emit(OpCodes.Stloc, returnLocal);
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].IsByRef)
                            {
                                ilGen.EmitLoadArg(i + 1);
                                ilGen.Emit(OpCodes.Ldloc, argsLocal);
                                ilGen.EmitInt(i);
                                ilGen.Emit(OpCodes.Ldelem_Ref);
                                ilGen.EmitConvertFromObject(parameters[i].GetElementType());
                                ilGen.EmitStRef(parameters[i]);
                            }
                        }
                        if (!method.IsVoid())
                        {
                            ilGen.Emit(OpCodes.Ldloc, returnLocal);
                            ilGen.EmitConvertFromObject(method.ReturnType);
                        }
                    }
                    ilGen.Emit(OpCodes.Ret);
                }

                void EmitProxyMethodBody()
                {
                    var ilGen = methodBuilder.GetILGenerator();
                    var activatorContext = ilGen.DeclareLocal(typeof(AspectActivatorContext));
                    var returnValue = default(LocalBuilder);

                    EmitInitializeMetaData(ilGen);

                    ilGen.Emit(OpCodes.Newobj, MethodUtils.AspectActivatorContextCtor);
                    ilGen.Emit(OpCodes.Stloc, activatorContext);

                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Ldfld, typeDesc.Fields[FieldBuilderUtils.ActivatorFactory]);
                    ilGen.Emit(OpCodes.Callvirt, MethodUtils.CreateAspectActivator);
                    ilGen.Emit(OpCodes.Ldloc, activatorContext);

                    EmitReturnVaule(ilGen);

                    if (method.ReturnType != typeof(void))
                    {
                        returnValue = ilGen.DeclareLocal(method.ReturnType);
                        ilGen.Emit(OpCodes.Stloc, returnValue);
                    }

                    var parameterTypes = method.GetParameterTypes();

                    if (parameterTypes.Any(x => x.IsByRef))
                    {
                        var parameters = ilGen.DeclareLocal(typeof(object[]));
                        ilGen.Emit(OpCodes.Ldloca, activatorContext);
                        ilGen.Emit(OpCodes.Call, MethodUtils.GetParameters);
                        ilGen.Emit(OpCodes.Stloc, parameters);
                        for (var i = 0; i < parameterTypes.Length; i++)
                        {
                            if (parameterTypes[i].IsByRef)
                            {
                                ilGen.EmitLoadArg(i + 1);
                                ilGen.Emit(OpCodes.Ldloc, parameters);
                                ilGen.EmitInt(i);
                                ilGen.Emit(OpCodes.Ldelem_Ref);
                                ilGen.EmitConvertFromObject(parameterTypes[i].GetElementType());
                                ilGen.EmitStRef(parameterTypes[i]);
                            }
                        }
                    }

                    if (returnValue != null)
                    {
                        ilGen.Emit(OpCodes.Ldloc, returnValue);
                    }
                    ilGen.Emit(OpCodes.Ret);
                }

                void EmitInitializeMetaData(ILGenerator ilGen)
                {
                    var serviceMethod = method;

                    var methodConstants = typeDesc.MethodConstants;

                    if (method.IsGenericMethodDefinition)
                    {
                        ilGen.EmitMethod(serviceMethod.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                        ilGen.EmitMethod(implementationMethod.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                        ilGen.EmitMethod(methodBuilder.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                    }
                    else
                    {
                        methodConstants.AddMethod($"service{serviceMethod.GetDisplayName()}", serviceMethod);
                        methodConstants.AddMethod($"impl{implementationMethod.GetDisplayName()}", implementationMethod);
                        methodConstants.AddMethod($"proxy{serviceMethod.GetDisplayName()}", methodBuilder);

                        methodConstants.LoadMethod(ilGen, $"service{serviceMethod.GetDisplayName()}");
                        methodConstants.LoadMethod(ilGen, $"impl{implementationMethod.GetDisplayName()}");
                        methodConstants.LoadMethod(ilGen, $"proxy{serviceMethod.GetDisplayName()}");
                    }

                    ilGen.EmitThis();
                    ilGen.Emit(OpCodes.Ldfld, typeDesc.Fields[FieldBuilderUtils.Target]);
                    ilGen.EmitThis();
                    var parameterTypes = method.GetParameterTypes();
                    if (parameterTypes.Length == 0)
                    {
                        ilGen.Emit(OpCodes.Ldnull);
                        return;
                    }
                    ilGen.EmitInt(parameterTypes.Length);
                    ilGen.Emit(OpCodes.Newarr, typeof(object));
                    for (var i = 0; i < parameterTypes.Length; i++)
                    {
                        ilGen.Emit(OpCodes.Dup);
                        ilGen.EmitInt(i);
                        ilGen.EmitLoadArg(i + 1);
                        if (parameterTypes[i].IsByRef)
                        {
                            ilGen.EmitLdRef(parameterTypes[i]);
                            ilGen.EmitConvertToObject(parameterTypes[i].GetElementType());
                        }
                        else
                        {
                            ilGen.EmitConvertToObject(parameterTypes[i]);
                        }
                        ilGen.Emit(OpCodes.Stelem_Ref);
                    }
                }

                void EmitReturnVaule(ILGenerator ilGen)
                {
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
                    else if (method.ReturnType == typeof(ValueTask))
                    {
                        ilGen.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvokeValueTask.MakeGenericMethod(typeof(object)));
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
                        var builder = DefineInterfaceProxyProperty(property, property.GetDisplayName(), implType, typeDesc);
                        DefineExplicitPropertyMethod(builder, property, implType, typeDesc);
                    }
                }
            }

            internal static void DefineClassProxyProperties(Type serviceType, Type implType, Type[] additionalInterfaces, TypeDesc typeDesc)
            {
                foreach (var property in serviceType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (property.IsVisibleAndVirtual())
                    {
                        var builder = DefineInterfaceProxyProperty(property, property.Name, implType, typeDesc);
                        DefineClassPropertyMethod(builder, property, implType, typeDesc);
                    }
                }
                foreach (var item in additionalInterfaces)
                {
                    foreach (var property in item.GetTypeInfo().DeclaredProperties)
                    {
                        var builder = DefineInterfaceProxyProperty(property, property.GetDisplayName(), implType, typeDesc);
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

        /// <summary>
        /// 参数构建工具
        /// </summary>
        private class ParameterBuilderUtils
        {
            public static void DefineParameters(MethodInfo targetMethod, MethodBuilder methodBuilder)
            {
                var parameters = targetMethod.GetParameters();
                if (parameters.Length > 0)
                {
                    const int paramOffset = 1; // 1
                    foreach (var parameter in parameters)
                    {
                        var parameterBuilder = methodBuilder.DefineParameter(parameter.Position + paramOffset, parameter.Attributes, parameter.Name);
                        // if (parameter.HasDefaultValue) // parameter.HasDefaultValue will throw a FormatException when parameter is DateTime type with default value
                        if (parameter.HasDefaultValueByAttributes())
                        {
                            // if (!(parameter.ParameterType.GetTypeInfo().IsValueType && parameter.DefaultValue == null)) 
                            // we can comment above line safely, and CopyDefaultValueConstant will handle this case.
                            // parameter.DefaultValue will throw a FormatException when parameter is DateTime type with default value
                            {
                                // parameterBuilder.SetConstant(parameter.DefaultValue);
                                try
                                {
                                    CopyDefaultValueConstant(from: parameter, to: parameterBuilder);
                                }
                                catch
                                {
                                    // Default value replication is a nice-to-have feature but not essential,
                                    // so if it goes wrong for one parameter, just continue.
                                }
                            }
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

            // Code from https://github.com/castleproject/Core/blob/master/src/Castle.Core/DynamicProxy/Generators/Emitters/MethodEmitter.cs
            private static void CopyDefaultValueConstant(ParameterInfo from, ParameterBuilder to)
            {
                object defaultValue;
                try
                {
                    defaultValue = from.DefaultValue;
                }
                catch (FormatException) when (from.ParameterType == typeof(DateTime))
                {
                    // This catch clause guards against a CLR bug that makes it impossible to query
                    // the default value of an optional DateTime parameter. For the CoreCLR, see
                    // https://github.com/dotnet/corefx/issues/26164.

                    // If this bug is present, it is caused by a `null` default value:
                    defaultValue = null;
                }
                catch (FormatException) when (from.ParameterType.GetTypeInfo().IsEnum)
                {
                    // This catch clause guards against a CLR bug that makes it impossible to query
                    // the default value of a (closed generic) enum parameter. For the CoreCLR, see
                    // https://github.com/dotnet/corefx/issues/29570.

                    // If this bug is present, it is caused by a `null` default value:
                    defaultValue = null;
                }

                if (defaultValue is Missing)
                {
                    // It is likely that we are reflecting over invalid metadata if we end up here.
                    // At this point, `to.Attributes` will have the `HasDefault` flag set. If we do
                    // not call `to.SetConstant`, that flag will be reset when creating the dynamic
                    // type, so `to` will at least end up having valid metadata. It is quite likely
                    // that the `Missing.Value` will still be reproduced because the `Parameter-
                    // Builder`'s `ParameterAttributes.Optional` is likely set. (If it isn't set,
                    // we'll be causing a default value of `DBNull.Value`, but there's nothing that
                    // can be done about that, short of recreating a new `ParameterBuilder`.)
                    return;
                }

                try
                {
                    to.SetConstant(defaultValue);
                }
                catch (ArgumentException)
                {
                    var parameterType = from.ParameterType;
                    var parameterNonNullableType = parameterType;
                    var isNullableType = parameterType.IsNullableType();

                    if (defaultValue == null)
                    {
                        if (isNullableType)
                        {
                            // This guards against a Mono bug that prohibits setting default value `null`
                            // for a `Nullable<T>` parameter. See https://github.com/mono/mono/issues/8504.
                            //
                            // If this bug is present, luckily we still get `null` as the default value if
                            // we do nothing more (which is probably itself yet another bug, as the CLR
                            // would "produce" a default value of `Missing.Value` in this situation).
                            return;
                        }
                        else if (parameterType.GetTypeInfo().IsValueType)
                        {
                            // This guards against a CLR bug that prohibits replicating `null` default
                            // values for non-nullable value types (which, despite the apparent type
                            // mismatch, is perfectly legal and something that the Roslyn compilers do).
                            // For the CoreCLR, see https://github.com/dotnet/corefx/issues/26184.

                            // If this bug is present, the best we can do is to not set the default value.
                            // This will cause a default value of `Missing.Value` (if `ParameterAttributes-
                            // .Optional` is set) or `DBNull.Value` (otherwise, unlikely).
                            return;
                        }
                    }
                    else if (isNullableType)
                    {
                        parameterNonNullableType = from.ParameterType.GetGenericArguments()[0];
                        if (parameterNonNullableType.GetTypeInfo().IsEnum || parameterNonNullableType.IsInstanceOfType(defaultValue))
                        {
                            // This guards against two bugs:
                            //
                            // * On the CLR and CoreCLR, a bug that makes it impossible to use `ParameterBuilder-
                            //   .SetConstant` on parameters of a nullable enum type. For CoreCLR, see
                            //   https://github.com/dotnet/coreclr/issues/17893.
                            //
                            //   If this bug is present, there is no way to faithfully reproduce the default
                            //   value. This will most likely cause a default value of `Missing.Value` or
                            //   `DBNull.Value`. (To better understand which of these, see comment above).
                            //
                            // * On Mono, a bug that performs a too-strict type check for nullable types. The
                            //   value passed to `ParameterBuilder.SetConstant` must have a type matching that
                            //   of the parameter precisely. See https://github.com/mono/mono/issues/8597.
                            //
                            //   If this bug is present, there's no way to reproduce the default value because
                            //   we cannot actually create a value of type `Nullable<>`.
                            return;
                        }
                    }

                    // Finally, we might have got here because the metadata constant simply doesn't match
                    // the parameter type exactly. Some code generators other than the .NET compilers
                    // might produce such metadata. Make a final attempt to coerce it to the required type:
                    try
                    {
                        var coercedDefaultValue = Convert.ChangeType(defaultValue, parameterNonNullableType, CultureInfo.InvariantCulture);
                        to.SetConstant(coercedDefaultValue);

                        return;
                    }
                    catch
                    {
                        // We don't care about the error thrown by an unsuccessful type coercion.
                    }

                    throw;
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

        /// <summary>
        /// 泛型参数生成工具
        /// </summary>
        private class GenericParameterUtils
        {
            /// <summary>
            /// 为泛型类型定义泛型参数
            /// </summary>
            /// <param name="targetType">泛型类型</param>
            /// <param name="typeBuilder">类型构建器</param>
            internal static void DefineGenericParameter(Type targetType, TypeBuilder typeBuilder)
            {
                if (!targetType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    return;
                }
                //获取泛型参数的类型数组
                var genericArguments = targetType.GetTypeInfo().GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
                var genericArgumentsBuilders = typeBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
                for (var index = 0; index < genericArguments.Length; index++)
                {
                    //设置泛型参数的方差特征和特殊约束
                    genericArgumentsBuilders[index].SetGenericParameterAttributes(ToClassGenericParameterAttributes(genericArguments[index].GenericParameterAttributes));
                    foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                    {
                        //基类约束,T继承约束规定的基类（SetBaseTypeConstraint：设置某类型必须继承的基类型，以替换为类型参数）
                        if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                        //接口约束,T实现约束规定的接口（SetInterfaceConstraints:设置一个类型必须实现的接口，以替换为类型参数）
                        if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                    }
                }
            }

            /// <summary>
            /// 为泛型方法定义泛型参数
            /// </summary>
            /// <param name="tergetMethod">泛型方法</param>
            /// <param name="methodBuilder">方法构建器</param>
            internal static void DefineGenericParameter(MethodInfo tergetMethod, MethodBuilder methodBuilder)
            {
                //非泛型方法直接退出
                if (!tergetMethod.IsGenericMethod)
                {
                    return;
                }
                //获取泛型方法的泛型参数类型数组
                var genericArguments = tergetMethod.GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
                var genericArgumentsBuilders = methodBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
                for (var index = 0; index < genericArguments.Length; index++)
                {
                    //指定泛型参数的约束
                    genericArgumentsBuilders[index].SetGenericParameterAttributes(genericArguments[index].GenericParameterAttributes);
                    foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                    {
                        if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                        if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                    }
                }
            }

            /// <summary>
            /// 处理泛型参数的修饰符
            /// </summary>
            /// <param name="attributes">描述对泛型类型或方法的泛型类型参数的约束</param>
            /// <returns>描述对泛型类型或方法的泛型类型参数的约束</returns>
            private static GenericParameterAttributes ToClassGenericParameterAttributes(GenericParameterAttributes attributes)
            {
                if (attributes == GenericParameterAttributes.None)
                {
                    return GenericParameterAttributes.None;
                }

                //SpecialConstraintMask:选择所有特殊约束标志的组合。 此值是使用逻辑“OR”合并后列标志得出的结果：DefaultConstructorConstraint、ReferenceTypeConstraint 和 NotNullableValueTypeConstraint。
                if (attributes.HasFlag(GenericParameterAttributes.SpecialConstraintMask))
                {
                    return GenericParameterAttributes.SpecialConstraintMask;
                }

                //** 非空值类型约束 T:struct **
                //NotNullableValueTypeConstraint:仅当一个类型是值类型且不可为 null 时，才能替代泛型类型参数
                if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
                {
                    return GenericParameterAttributes.NotNullableValueTypeConstraint;
                }

                //** 无参构造与引用类型约束 T: class, new() **
                //ReferenceTypeConstraint:仅当一个类型是引用类型时，才能替代泛型类型参数
                //DefaultConstructorConstraint:仅当一个类型具有无参数的构造函数时，才能替代泛型类型参数
                if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                {
                    return GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
                }
                //T: class
                if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
                {
                    return GenericParameterAttributes.ReferenceTypeConstraint;
                }
                //T: new()
                if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                {
                    return GenericParameterAttributes.DefaultConstructorConstraint;
                }
                return GenericParameterAttributes.None;
            }
        }

        /// <summary>
        /// 自定义特性构建工具
        /// </summary>
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
                    var constructorArgs = customAttributeData.ConstructorArguments
                        .Select(ReadAttributeValue)
                        .ToArray();
                    var namedProperties = customAttributeData.NamedArguments
                            .Where(n => !n.IsField)
                            .Select(n => attributeTypeInfo.GetProperty(n.MemberName))
                            .ToArray();
                    var propertyValues = customAttributeData.NamedArguments
                             .Where(n => !n.IsField)
                             .Select(n => ReadAttributeValue(n.TypedValue))
                             .ToArray();
                    var namedFields = customAttributeData.NamedArguments.Where(n => n.IsField)
                             .Select(n => attributeTypeInfo.GetField(n.MemberName))
                             .ToArray();
                    var fieldValues = customAttributeData.NamedArguments.Where(n => n.IsField)
                             .Select(n => ReadAttributeValue(n.TypedValue))
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

            private static object ReadAttributeValue(CustomAttributeTypedArgument argument)
            {
                var value = argument.Value;
                if (argument.ArgumentType.GetTypeInfo().IsArray == false)
                {
                    return value;
                }
                //special case for handling arrays in attributes
                //the actual type of "value" is ReadOnlyCollection<CustomAttributeTypedArgument>.
                var arguments = ((IEnumerable<CustomAttributeTypedArgument>)value)
                    .Select(m => m.Value)
                    .ToArray();
                return arguments;
            }
        }

        /// <summary>
        /// 字段构建工具
        /// </summary>
        private class FieldBuilderUtils
        {
            public const string ActivatorFactory = "_activatorFactory";
            public const string Target = "_implementation";

            /// <summary>
            /// 定义一个字段
            /// </summary>
            /// <param name="targetType">类型</param>
            /// <param name="typeBuilder">TypeBuilder</param>
            /// <returns>FieldTable</returns>
            public static FieldTable DefineFields(Type targetType, TypeBuilder typeBuilder)
            {
                var fieldTable = new FieldTable();
                //定义两个字段并保存于FieldTable
                fieldTable[ActivatorFactory] = typeBuilder.DefineField(ActivatorFactory, typeof(IAspectActivatorFactory), FieldAttributes.Private);
                fieldTable[Target] = typeBuilder.DefineField(Target, targetType, FieldAttributes.Private);
                return fieldTable;
            }
        }

        /// <summary>
        /// 用于存取字段
        /// </summary>
        private class FieldTable
        {
            private readonly Dictionary<string, FieldBuilder> _table = new Dictionary<string, FieldBuilder>();

            /// <summary>
            /// 字段名称的索引器
            /// </summary>
            /// <param name="fieldName">字段名称</param>
            /// <returns>FieldBuilder</returns>
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

        /// <summary>
        /// 类型描述
        /// </summary>
        private class TypeDesc
        {
            public TypeBuilder Builder { get; }

            public FieldTable Fields { get; }

            public MethodConstantTable MethodConstants { get; }

            public Dictionary<string, object> Properties { get; }

            public Type ServiceType { get; }

            public TypeDesc(Type serviceType, TypeBuilder typeBuilder, FieldTable fields, MethodConstantTable methodConstants)
            {
                ServiceType = serviceType;
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