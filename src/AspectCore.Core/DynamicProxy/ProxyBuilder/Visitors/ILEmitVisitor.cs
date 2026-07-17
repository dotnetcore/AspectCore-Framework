using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using AspectCore.Extensions.Reflection.Emit;
using AspectCore.Utils;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;

namespace AspectCore.DynamicProxy.ProxyBuilder.Visitors
{
    internal class ILEmitVisitor : IProxyBuilderVisitor
    {
        private readonly ILEmitVisitorContext _ctx;

        public ILEmitVisitor(ILEmitVisitorContext context)
        {
            _ctx = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Type[] VisitAll(ProxyTypeNode[] nodes)
        {
            var results = new Type[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
                results[i] = VisitProxyType(nodes[i]);
            return results;
        }

        public Type VisitProxyType(ProxyTypeNode node)
        {
            var typeBuilder = _ctx.ModuleBuilder.DefineType(
                node.Name,
                node.ProxyKind == ProxyKind.InterfaceImpl
                    ? TypeAttributes.Class | TypeAttributes.Public
                    : TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed,
                node.ParentType,
                node.Interfaces);

            _ctx.TypeBuilder = typeBuilder;
            _ctx.ServiceType = node.ServiceType;
            _ctx.Fields.Clear();

            // Generic parameters
            if (node.GenericParameters.Count > 0)
                DefineGenericParameters(typeBuilder, node.GenericParameters);

            // Attributes
            foreach (var attr in node.Attributes)
                VisitAttribute(attr);

            // Fields
            foreach (var field in node.Fields)
                VisitField(field);

            // Method constants
            if (node.ProxyKind != ProxyKind.InterfaceImpl)
            {
                _ctx.MethodConstants = new MethodConstantTable(typeBuilder);
                foreach (var mc in node.MethodConstants)
                    VisitMethodConstant(mc);
            }

            // Constructors
            foreach (var ctor in node.Constructors)
                VisitConstructor(ctor);

            // Methods
            foreach (var method in node.Methods)
                VisitMethod(method);

            // Properties
            foreach (var prop in node.Properties)
                VisitProperty(prop);

            // Compile
            if (_ctx.MethodConstants != null)
                _ctx.MethodConstants.Compile();

            _ctx.MethodConstants = null;
            return typeBuilder.CreateTypeInfo().AsType();
        }

        public void VisitField(FieldNode node)
        {
            var field = _ctx.TypeBuilder.DefineField(node.Name, node.FieldType, node.Accessibility);
            _ctx.Fields[node.Name] = field;
        }

        public void VisitConstructor(ConstructorNode node)
        {
            switch (node.Kind)
            {
                case ConstructorKind.DefaultObjectCtor:
                    EmitDefaultObjectCtor(node);
                    break;
                case ConstructorKind.InterfaceProxyCtorWithFactory:
                    EmitInterfaceProxyCtorWithFactory(node);
                    break;
                case ConstructorKind.InterfaceProxyCtorWithFactoryAndTarget:
                    EmitInterfaceProxyCtorWithFactoryAndTarget(node);
                    break;
                case ConstructorKind.ClassProxyCtorFromBase:
                    EmitClassProxyCtor(node);
                    break;
            }
        }

        private void EmitDefaultObjectCtor(ConstructorNode node)
        {
            var ctorBuilder = _ctx.TypeBuilder.DefineConstructor(
                MethodAttributes.Public, MethodUtils.ObjectCtor.CallingConvention, Type.EmptyTypes);
            var il = ctorBuilder.GetILGenerator();
            il.EmitThis();
            il.Emit(OpCodes.Call, MethodUtils.ObjectCtor);
            il.Emit(OpCodes.Ret);
        }

        private void EmitInterfaceProxyCtorWithFactory(ConstructorNode node)
        {
            var ctorBuilder = _ctx.TypeBuilder.DefineConstructor(
                MethodAttributes.Public, MethodUtils.ObjectCtor.CallingConvention,
                new[] { typeof(IAspectActivatorFactory) });

            ctorBuilder.DefineParameter(1, ParameterAttributes.None, "_activatorFactory");

            var il = ctorBuilder.GetILGenerator();
            il.EmitThis();
            il.Emit(OpCodes.Call, MethodUtils.ObjectCtor);

            // Field assignments
            foreach (var fa in node.FieldAssignments)
            {
                il.EmitThis();
                il.EmitLoadArg(fa.SourceArgIndex);
                il.Emit(OpCodes.Stfld, _ctx.Fields[fa.FieldName]);
            }

            // Create target instance: this._implementation = new StubImplType()
            if (node.TargetCreation != null)
            {
                il.EmitThis();
                il.Emit(OpCodes.Newobj, node.TargetCreation.ImplType.GetTypeInfo().GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stfld, _ctx.Fields[node.TargetCreation.TargetFieldName]);
            }

            il.Emit(OpCodes.Ret);
        }

        private void EmitInterfaceProxyCtorWithFactoryAndTarget(ConstructorNode node)
        {
            var ctorBuilder = _ctx.TypeBuilder.DefineConstructor(
                MethodAttributes.Public, MethodUtils.ObjectCtor.CallingConvention, node.ParameterTypes);

            ctorBuilder.DefineParameter(1, ParameterAttributes.None, "_activatorFactory");
            ctorBuilder.DefineParameter(2, ParameterAttributes.None, "_implementation");

            var il = ctorBuilder.GetILGenerator();
            il.EmitThis();
            il.Emit(OpCodes.Call, MethodUtils.ObjectCtor);

            foreach (var fa in node.FieldAssignments)
            {
                il.EmitThis();
                il.EmitLoadArg(fa.SourceArgIndex);
                il.Emit(OpCodes.Stfld, _ctx.Fields[fa.FieldName]);
            }

            il.Emit(OpCodes.Ret);
        }

        private void EmitClassProxyCtor(ConstructorNode node)
        {
            var (requiredCustomModifiers, optionalCustomModifiers) = GetConstructorParameterCustomModifiers(node);
            var ctorBuilder = _ctx.TypeBuilder.DefineConstructor(
                node.MethodAttributes,
                node.CallingConvention,
                node.ParameterTypes,
                requiredCustomModifiers,
                optionalCustomModifiers);

            // Attributes
            foreach (var attr in node.Attributes)
                SetCustomAttribute(ctorBuilder, attr);

            // Parameters: first param is IAspectActivatorFactory (offset 1), then base ctor params (offset 2+)
            ctorBuilder.DefineParameter(1, ParameterAttributes.None, "aspectContextFactory");
            if (node.Parameters.Count > 0)
            {
                var paramOffset = 2;
                for (var i = 0; i < node.Parameters.Count; i++)
                {
                    var p = node.Parameters[i];
                    var pb = ctorBuilder.DefineParameter(i + paramOffset, p.Attributes, p.Name);
                    if (p.HasDefaultValue)
                    {
                        try { pb.SetConstant(p.DefaultValue); } catch { }
                    }
                    foreach (var attr in p.CustomAttributes)
                        SetCustomAttribute(pb, attr);
                }
            }

            var il = ctorBuilder.GetILGenerator();

            // this._activatorFactory = arg1
            il.EmitThis();
            il.EmitLoadArg(1);
            il.Emit(OpCodes.Stfld, _ctx.Fields["_activatorFactory"]);

            // this._implementation = this (class proxy wraps itself)
            il.EmitThis();
            il.EmitThis();
            il.Emit(OpCodes.Stfld, _ctx.Fields["_implementation"]);

            // base(arg2, arg3, ...)
            il.EmitThis();
            var totalParams = node.ParameterTypes.Length;
            for (var i = 2; i <= totalParams; i++)
                il.EmitLoadArg(i);
            il.Emit(OpCodes.Call, node.BaseConstructor);

            il.Emit(OpCodes.Ret);
        }

        public void VisitMethod(MethodNode node)
        {
            var parameterInfos = node.ServiceMethod.GetParameters();
            var methodBuilder = _ctx.TypeBuilder.DefineMethod(
                node.Name, node.MethodAttributes,
                node.ServiceMethod.CallingConvention,
                node.ServiceMethod.ReturnType,
                GetRequiredCustomModifiers(node.ServiceMethod.ReturnParameter),
                GetOptionalCustomModifiers(node.ServiceMethod.ReturnParameter),
                parameterInfos.Select(p => p.ParameterType).ToArray(),
                parameterInfos.Select(GetRequiredCustomModifiers).ToArray(),
                parameterInfos.Select(GetOptionalCustomModifiers).ToArray());

            _ctx.CurrentMethodBuilder = methodBuilder;

            // Generic parameters
            if (node.GenericParameters.Count > 0)
                DefineMethodGenericParameters(methodBuilder, node.GenericParameters);

            // Attributes
            foreach (var attr in node.Attributes)
                SetCustomAttribute(methodBuilder, attr);

            // Parameters
            EmitMethodParameters(node, methodBuilder);

            // Override
            if (node.OverridesMethod != null)
                _ctx.TypeBuilder.DefineMethodOverride(methodBuilder, node.OverridesMethod);

            // Body
            _ctx.CurrentILGenerator = methodBuilder.GetILGenerator();
            node.Body.Accept(this);
        }

        private void EmitMethodParameters(MethodNode node, MethodBuilder methodBuilder)
        {
            if (node.Parameters.Count == 0) return;

            // Regular parameters (position > 0) come first, return parameter (position 0) last
            foreach (var param in node.Parameters)
            {
                if (param.Position < 0)
                {
                    // Return parameter
                    var pb = methodBuilder.DefineParameter(0, param.Attributes, param.Name);
                    foreach (var attr in param.CustomAttributes)
                        SetCustomAttribute(pb, attr);
                }
                else
                {
                    var pb = methodBuilder.DefineParameter(param.Position + 1, param.Attributes, param.Name);
                    if (param.HasDefaultValue)
                    {
                        try
                        {
                            CopyDefaultValueConstant(param, pb);
                        }
                        catch { }
                    }
                    foreach (var attr in param.CustomAttributes)
                        SetCustomAttribute(pb, attr);
                }
            }
        }

        public void VisitProperty(PropertyNode node)
        {
            // Backing field for stub properties
            if (node.BackingField != null && !_ctx.Fields.ContainsKey(node.BackingField.Name))
            {
                var field = _ctx.TypeBuilder.DefineField(node.BackingField.Name, node.BackingField.FieldType, node.BackingField.Accessibility);
                _ctx.Fields[node.BackingField.Name] = field;
            }

            var propertyBuilder = _ctx.TypeBuilder.DefineProperty(
                node.Name,
                node.PropertyAttributes,
                node.PropertyType,
                node.RequiredCustomModifiers,
                node.OptionalCustomModifiers,
                Type.EmptyTypes,
                null,
                null);

            foreach (var attr in node.Attributes)
                SetCustomAttribute(propertyBuilder, attr);

            if (node.GetMethod != null)
            {
                VisitMethod(node.GetMethod);
                propertyBuilder.SetGetMethod(_ctx.CurrentMethodBuilder);
            }

            if (node.SetMethod != null)
            {
                VisitMethod(node.SetMethod);
                propertyBuilder.SetSetMethod(_ctx.CurrentMethodBuilder);
            }
        }

        public void VisitParameter(ParameterNode node)
        {
            // Handled inline in EmitMethodParameters / EmitClassProxyCtor
        }

        public void VisitGenericParameter(GenericParameterNode node)
        {
            // Handled inline in DefineGenericParameters
        }

        public void VisitAttribute(AttributeNode node)
        {
            var builder = node.IsMarker
                ? BuildCustomAttribute(node.MarkerAttributeType)
                : BuildCustomAttribute(node.CustomAttributeData);
            _ctx.TypeBuilder.SetCustomAttribute(builder);
        }

        public void VisitMethodConstant(MethodConstantNode node)
        {
            // For proxy methods, the MethodBuilder is not yet created; store null and update during body emit
            _ctx.MethodConstants.AddMethod(node.Key, node.Method);
        }

        // --- Method body visitors ---

        public void VisitDirectDelegationBody(DirectDelegationBody node)
        {
            var il = _ctx.CurrentILGenerator;
            var parameters = node.ServiceMethod.GetParameterTypes();

            il.EmitThis();
            il.Emit(OpCodes.Ldfld, _ctx.Fields[node.TargetFieldName]);
            for (int i = 1; i <= parameters.Length; i++)
                il.EmitLoadArg(i);

            var callOpCode = node.IsCallvirt ? OpCodes.Callvirt : OpCodes.Call;
            il.Emit(callOpCode, node.TargetMethod);
            il.Emit(OpCodes.Ret);
        }

        public void VisitReflectorDelegationBody(ReflectorDelegationBody node)
        {
            var il = _ctx.CurrentILGenerator;
            var parameters = node.ServiceMethod.GetParameterTypes();

            var reflectorLocal = il.DeclareLocal(typeof(MethodReflector));
            var argsLocal = il.DeclareLocal(typeof(object[]));
            var returnLocal = il.DeclareLocal(typeof(object));

            il.EmitMethod(node.ImplementationMethod);
            il.Emit(OpCodes.Call, MethodUtils.GetMethodReflector);
            il.Emit(OpCodes.Stloc, reflectorLocal);

            il.EmitInt(parameters.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < parameters.Length; i++)
            {
                il.Emit(OpCodes.Dup);
                il.EmitInt(i);
                il.EmitLoadArg(i + 1);
                if (parameters[i].IsByRef)
                {
                    il.EmitLdRef(parameters[i]);
                    il.EmitConvertToObject(parameters[i].GetElementType());
                }
                else
                {
                    il.EmitConvertToObject(parameters[i]);
                }
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Stloc, argsLocal);

            il.Emit(OpCodes.Ldloc, reflectorLocal);
            il.EmitThis();
            il.Emit(OpCodes.Ldloc, argsLocal);
            il.Emit(OpCodes.Callvirt, MethodUtils.ReflectorInvoke);
            il.Emit(OpCodes.Stloc, returnLocal);

            // Write back byref parameters
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsByRef)
                {
                    var byrefToType = parameters[i].GetElementType();
                    il.EmitLoadArg(i + 1);
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.EmitInt(i);
                    il.Emit(OpCodes.Ldelem_Ref);
                    il.EmitConvertFromObject(byrefToType);
                    il.EmitStRef(byrefToType);
                }
            }

            if (!node.ServiceMethod.IsVoid())
            {
                il.Emit(OpCodes.Ldloc, returnLocal);
                il.EmitConvertFromObject(node.ServiceMethod.ReturnType);
            }

            il.Emit(OpCodes.Ret);
        }

        public void VisitAspectActivatorBody(AspectActivatorBody node)
        {
            var il = _ctx.CurrentILGenerator;
            var methodBuilder = _ctx.CurrentMethodBuilder;
            var activatorContext = il.DeclareLocal(typeof(AspectActivatorContext));
            LocalBuilder returnValue = null;

            // Emit metadata initialization
            EmitInitializeMetaData(il, node, methodBuilder);

            il.Emit(OpCodes.Newobj, MethodUtils.AspectActivatorContextCtor);
            il.Emit(OpCodes.Stloc, activatorContext);

            il.EmitThis();
            il.Emit(OpCodes.Ldfld, _ctx.Fields["_activatorFactory"]);
            il.Emit(OpCodes.Callvirt, MethodUtils.CreateAspectActivator);
            il.Emit(OpCodes.Ldloc, activatorContext);

            EmitReturnValue(il, node);

            if (node.ReturnKind != ReturnKind.Void)
            {
                returnValue = il.DeclareLocal(node.ServiceMethod.ReturnType);
                il.Emit(OpCodes.Stloc, returnValue);
            }

            // Write back byref parameters
            var parameterTypes = node.ServiceMethod.GetParameterTypes();
            if (parameterTypes.Any(x => x.IsByRef))
            {
                var parameters = il.DeclareLocal(typeof(object[]));
                il.Emit(OpCodes.Ldloca, activatorContext);
                il.Emit(OpCodes.Call, MethodUtils.GetParameters);
                il.Emit(OpCodes.Stloc, parameters);
                for (var i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameterTypes[i].IsByRef)
                    {
                        var byrefToType = parameterTypes[i].GetElementType();
                        il.EmitLoadArg(i + 1);
                        il.Emit(OpCodes.Ldloc, parameters);
                        il.EmitInt(i);
                        il.Emit(OpCodes.Ldelem_Ref);
                        il.EmitConvertFromObject(byrefToType);
                        il.EmitStRef(byrefToType);
                    }
                }
            }

            if (returnValue != null)
                il.Emit(OpCodes.Ldloc, returnValue);

            il.Emit(OpCodes.Ret);
        }

        private void EmitInitializeMetaData(ILGenerator il, AspectActivatorBody node, MethodBuilder methodBuilder)
        {
            var serviceMethod = node.ServiceMethod;
            var implementationMethod = node.ImplementationMethod;
            var predicateMethod = node.PredicateMethod;

            if (node.IsGeneric)
            {
                il.EmitMethod(serviceMethod.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                il.EmitMethod(implementationMethod.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                il.EmitMethod(methodBuilder.MakeGenericMethod(methodBuilder.GetGenericArguments()));
                il.EmitMethod(predicateMethod.MakeGenericMethod(methodBuilder.GetGenericArguments()));
            }
            else
            {
                var serviceKey = $"service{serviceMethod.GetDisplayName()}";
                var implKey = $"impl{implementationMethod.GetDisplayName()}";
                var proxyKey = $"proxy{serviceMethod.GetDisplayName()}";
                var predicateKey = $"predicate{predicateMethod.GetDisplayName()}";

                // Store the proxy method builder in constants
                _ctx.MethodConstants.AddMethod(proxyKey, methodBuilder);

                _ctx.MethodConstants.LoadMethod(il, serviceKey);
                _ctx.MethodConstants.LoadMethod(il, implKey);
                _ctx.MethodConstants.LoadMethod(il, proxyKey);
                _ctx.MethodConstants.LoadMethod(il, predicateKey);
            }

            il.EmitThis();
            il.Emit(OpCodes.Ldfld, _ctx.Fields["_implementation"]);
            il.EmitThis();

            var parameterTypes = serviceMethod.GetParameterTypes();
            if (parameterTypes.Length == 0)
            {
                il.Emit(OpCodes.Ldnull);
                return;
            }

            il.EmitInt(parameterTypes.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < parameterTypes.Length; i++)
            {
                il.Emit(OpCodes.Dup);
                il.EmitInt(i);
                il.EmitLoadArg(i + 1);
                if (parameterTypes[i].IsByRef)
                {
                    il.EmitLdRef(parameterTypes[i]);
                    il.EmitConvertToObject(parameterTypes[i].GetElementType());
                }
                else
                {
                    il.EmitConvertToObject(parameterTypes[i]);
                }
                il.Emit(OpCodes.Stelem_Ref);
            }
        }

        private void EmitReturnValue(ILGenerator il, AspectActivatorBody node)
        {
            switch (node.ReturnKind)
            {
                case ReturnKind.Void:
                    il.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvoke.MakeGenericMethod(typeof(object)));
                    il.Emit(OpCodes.Pop);
                    break;
                case ReturnKind.Task:
                    il.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvokeTask.MakeGenericMethod(typeof(object)));
                    break;
                case ReturnKind.TaskOfT:
                    var taskReturnType = node.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                    il.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvokeTask.MakeGenericMethod(taskReturnType));
                    break;
                case ReturnKind.ValueTask:
                    il.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvokeValueTask.MakeGenericMethod(typeof(object)));
                    break;
                case ReturnKind.ValueTaskOfT:
                    var vtReturnType = node.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                    il.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvokeValueTask.MakeGenericMethod(vtReturnType));
                    break;
                case ReturnKind.Sync:
                    il.Emit(OpCodes.Callvirt, MethodUtils.AspectActivatorInvoke.MakeGenericMethod(node.ReturnType));
                    break;
            }
        }

        public void VisitStubBody(StubBody node)
        {
            var il = _ctx.CurrentILGenerator;
            if (node.ReturnType != typeof(void))
                il.EmitDefault(node.ReturnType);
            il.Emit(OpCodes.Ret);
        }

        public void VisitBackingFieldGetBody(BackingFieldGetBody node)
        {
            var il = _ctx.CurrentILGenerator;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, _ctx.Fields[node.FieldName]);
            il.Emit(OpCodes.Ret);
        }

        public void VisitBackingFieldSetBody(BackingFieldSetBody node)
        {
            var il = _ctx.CurrentILGenerator;
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stfld, _ctx.Fields[node.FieldName]);
            il.Emit(OpCodes.Ret);
        }

        // --- Helper methods ---

        private void DefineGenericParameters(TypeBuilder typeBuilder, IReadOnlyList<GenericParameterNode> nodes)
        {
            var names = nodes.Select(n => n.Name).ToArray();
            var builders = typeBuilder.DefineGenericParameters(names);
            for (int i = 0; i < nodes.Count; i++)
            {
                builders[i].SetGenericParameterAttributes(nodes[i].Constraints);
                if (nodes[i].BaseTypeConstraint != null)
                    builders[i].SetBaseTypeConstraint(nodes[i].BaseTypeConstraint);
                if (nodes[i].InterfaceConstraints.Length > 0)
                    builders[i].SetInterfaceConstraints(nodes[i].InterfaceConstraints);
                foreach (var attribute in nodes[i].Attributes)
                    SetCustomAttribute(builders[i], attribute);
            }
        }

        private void DefineMethodGenericParameters(MethodBuilder methodBuilder, IReadOnlyList<GenericParameterNode> nodes)
        {
            var names = nodes.Select(n => n.Name).ToArray();
            var builders = methodBuilder.DefineGenericParameters(names);
            for (int i = 0; i < nodes.Count; i++)
            {
                builders[i].SetGenericParameterAttributes(nodes[i].Constraints);
                if (nodes[i].BaseTypeConstraint != null)
                    builders[i].SetBaseTypeConstraint(nodes[i].BaseTypeConstraint);
                if (nodes[i].InterfaceConstraints.Length > 0)
                    builders[i].SetInterfaceConstraints(nodes[i].InterfaceConstraints);
                foreach (var attribute in nodes[i].Attributes)
                    SetCustomAttribute(builders[i], attribute);
            }
        }

        private CustomAttributeBuilder BuildCustomAttribute(Type attributeType)
        {
            return new CustomAttributeBuilder(
                attributeType.GetTypeInfo().GetConstructor(Type.EmptyTypes),
                ArrayUtils.Empty<object>());
        }

        private CustomAttributeBuilder BuildCustomAttribute(CustomAttributeData data)
        {
            // For generic attribute types (C# 11 feature: [SomeAttribute<T>]),
            // data.AttributeType is already the closed generic type. We resolve the
            // constructor from the closed generic type directly to ensure the
            // CustomAttributeBuilder receives a constructor compatible with the
            // closed type, rather than one from the open generic definition.
            var attributeType = data.AttributeType;
            var constructor = data.Constructor;

            if (attributeType.IsGenericType)
            {
                var parameterTypes = data.ConstructorArguments.Select(a => a.ArgumentType).ToArray();
                var resolvedConstructor = attributeType.GetConstructor(parameterTypes);
                if (resolvedConstructor != null)
                {
                    constructor = resolvedConstructor;
                }
            }

            if (data.NamedArguments != null)
            {
                var attributeTypeInfo = attributeType.GetTypeInfo();
                var constructorArgs = data.ConstructorArguments.Select(ReadAttributeValue).ToArray();
                var namedProperties = data.NamedArguments.Where(n => !n.IsField).Select(n => attributeTypeInfo.GetProperty(n.MemberName)).Where(p => p != null).ToArray();
                var propertyValues = data.NamedArguments.Where(n => !n.IsField && attributeTypeInfo.GetProperty(n.MemberName) != null).Select(n => ReadAttributeValue(n.TypedValue)).ToArray();
                var namedFields = data.NamedArguments.Where(n => n.IsField).Select(n => attributeTypeInfo.GetField(n.MemberName)).Where(f => f != null).ToArray();
                var fieldValues = data.NamedArguments.Where(n => n.IsField && attributeTypeInfo.GetField(n.MemberName) != null).Select(n => ReadAttributeValue(n.TypedValue)).ToArray();
                return new CustomAttributeBuilder(constructor, constructorArgs, namedProperties, propertyValues, namedFields, fieldValues);
            }

            return new CustomAttributeBuilder(constructor, data.ConstructorArguments.Select(c => c.Value).ToArray());
        }

        private static object ReadAttributeValue(CustomAttributeTypedArgument argument)
        {
            var value = argument.Value;
            if (!argument.ArgumentType.GetTypeInfo().IsArray)
                return value;
            return ((IEnumerable<CustomAttributeTypedArgument>)value).Select(m => m.Value).ToArray();
        }

        private void SetCustomAttribute(TypeBuilder builder, AttributeNode node)
        {
            builder.SetCustomAttribute(node.IsMarker ? BuildCustomAttribute(node.MarkerAttributeType) : BuildCustomAttribute(node.CustomAttributeData));
        }

        private void SetCustomAttribute(GenericTypeParameterBuilder builder, AttributeNode node)
        {
            builder.SetCustomAttribute(node.IsMarker ? BuildCustomAttribute(node.MarkerAttributeType) : BuildCustomAttribute(node.CustomAttributeData));
        }

        private void SetCustomAttribute(MethodBuilder builder, AttributeNode node)
        {
            builder.SetCustomAttribute(node.IsMarker ? BuildCustomAttribute(node.MarkerAttributeType) : BuildCustomAttribute(node.CustomAttributeData));
        }

        private void SetCustomAttribute(ConstructorBuilder builder, AttributeNode node)
        {
            builder.SetCustomAttribute(node.IsMarker ? BuildCustomAttribute(node.MarkerAttributeType) : BuildCustomAttribute(node.CustomAttributeData));
        }

        private void SetCustomAttribute(PropertyBuilder builder, AttributeNode node)
        {
            builder.SetCustomAttribute(node.IsMarker ? BuildCustomAttribute(node.MarkerAttributeType) : BuildCustomAttribute(node.CustomAttributeData));
        }

        private void SetCustomAttribute(ParameterBuilder builder, AttributeNode node)
        {
            builder.SetCustomAttribute(node.IsMarker ? BuildCustomAttribute(node.MarkerAttributeType) : BuildCustomAttribute(node.CustomAttributeData));
        }

        private static Type[] GetRequiredCustomModifiers(ParameterInfo parameter)
        {
            try
            {
                return parameter?.GetRequiredCustomModifiers() ?? Type.EmptyTypes;
            }
            catch
            {
                return Type.EmptyTypes;
            }
        }

        private static Type[] GetOptionalCustomModifiers(ParameterInfo parameter)
        {
            try
            {
                return parameter?.GetOptionalCustomModifiers() ?? Type.EmptyTypes;
            }
            catch
            {
                return Type.EmptyTypes;
            }
        }

        private static (Type[][] Required, Type[][] Optional) GetConstructorParameterCustomModifiers(ConstructorNode node)
        {
            var required = node.ParameterTypes.Select(_ => Type.EmptyTypes).ToArray();
            var optional = node.ParameterTypes.Select(_ => Type.EmptyTypes).ToArray();

            if (node.BaseConstructor == null)
            {
                return (required, optional);
            }

            var baseParameters = node.BaseConstructor.GetParameters();
            var offset = node.ParameterTypes.Length - baseParameters.Length;
            if (offset < 0)
            {
                return (required, optional);
            }

            for (var i = 0; i < baseParameters.Length; i++)
            {
                required[i + offset] = GetRequiredCustomModifiers(baseParameters[i]);
                optional[i + offset] = GetOptionalCustomModifiers(baseParameters[i]);
            }

            return (required, optional);
        }

        // Copied from original ParameterBuilderUtils.CopyDefaultValueConstant
        private static void CopyDefaultValueConstant(ParameterNode from, ParameterBuilder to)
        {
            var defaultValue = from.DefaultValue;

            if (defaultValue is System.Reflection.Missing)
                return;

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
                    if (isNullableType || parameterType.IsValueType)
                        return;
                }
                else if (isNullableType)
                {
                    parameterNonNullableType = parameterType.GetGenericArguments()[0];
                    if (parameterNonNullableType.IsEnum || parameterNonNullableType.IsInstanceOfType(defaultValue))
                        return;
                }

                try
                {
                    var coercedDefaultValue = Convert.ChangeType(defaultValue, parameterNonNullableType, CultureInfo.InvariantCulture);
                    to.SetConstant(coercedDefaultValue);
                    return;
                }
                catch { }

                throw;
            }
        }
    }
}
