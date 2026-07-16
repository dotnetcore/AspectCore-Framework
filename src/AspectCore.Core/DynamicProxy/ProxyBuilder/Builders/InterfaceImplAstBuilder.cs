using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Utils;
using AspectCore.Extensions.Reflection;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;
using AspectCore.Extensions;

namespace AspectCore.DynamicProxy.ProxyBuilder.Builders
{
    internal class InterfaceImplBuilder : IProxyTypeBuilder
    {
        private readonly string _implName;
        private readonly string _proxyName;
        private readonly Type _interfaceType;
        private readonly Type[] _additionalInterfaces;
        private readonly IAspectValidator _aspectValidator;

        public InterfaceImplBuilder(
            string implName,
            string proxyName,
            Type interfaceType,
            Type[] additionalInterfaces,
            IAspectValidator aspectValidator)
        {
            _implName = implName;
            _proxyName = proxyName;
            _interfaceType = interfaceType;
            _additionalInterfaces = additionalInterfaces;
            _aspectValidator = aspectValidator;
        }

        public ProxyTypeNode[] Build()
        {
            var interfaceTypes = new[] { _interfaceType }.Concat(_additionalInterfaces).Distinct().ToArray();
            var stubNode = BuildStubType(interfaceTypes);
            var proxyNode = BuildProxyType(interfaceTypes);
            return new[] { stubNode, proxyNode };
        }

        public ProxyTypeNode BuildStubOnly()
        {
            var interfaceTypes = new[] { _interfaceType }.Concat(_additionalInterfaces).Distinct().ToArray();
            return BuildStubType(interfaceTypes);
        }

        public ProxyTypeNode BuildProxyOnly(string proxyName, Type stubImplType)
        {
            var interfaceTypes = new[] { _interfaceType }.Concat(_additionalInterfaces).Distinct().ToArray();
            return BuildProxyTypeWithStub(proxyName, stubImplType, interfaceTypes);
        }

        private ProxyTypeNode BuildStubType(Type[] interfaceTypes)
        {
            var genericParams = GenericParameterNodeFactory.FromType(_interfaceType);

            var constructor = new ConstructorNode(
                ConstructorKind.DefaultObjectCtor,
                MethodAttributes.Public,
                MethodUtils.ObjectCtor.CallingConvention,
                Type.EmptyTypes,
                baseConstructor: null,
                parameters: null,
                attributes: null,
                fieldAssignments: null,
                targetCreation: null);

            var methods = new List<MethodNode>();
            var properties = new List<PropertyNode>();

            foreach (var iface in interfaceTypes)
            {
                foreach (var method in iface.GetTypeInfo().DeclaredMethods.Where(x => !x.IsPropertyBinding()))
                {
                    if (!method.IsAbstract) continue;
                    methods.Add(BuildStubMethod(method));
                }

                foreach (var property in iface.GetTypeInfo().DeclaredProperties)
                {
                    properties.Add(BuildStubProperty(property));
                }
            }

            return new ProxyTypeNode(
                _implName,
                ProxyKind.InterfaceImpl,
                _interfaceType,
                typeof(object),
                interfaceTypes,
                genericParams,
                attributes: null,
                fields: null,
                new[] { constructor },
                methods,
                properties,
                methodConstants: null);
        }

        private static MethodNode BuildStubMethod(MethodInfo method)
        {
            var genericParameters = GenericParameterNodeFactory.FromMethod(method);
            var parameters = ParameterNodeFactory.FromMethod(method);
            var attributes = AttributeNodeFactory.FromCustomAttributes(method.CustomAttributes);

            return new MethodNode(
                method,
                implementationMethod: null,
                method.Name,
                MethodBuilderConstants.InterfaceMethodAttributes,
                new StubBody(method.ReturnType),
                parameters,
                genericParameters,
                attributes,
                overridesMethod: method,
                predicateMethod: method);
        }

        private static PropertyNode BuildStubProperty(PropertyInfo property)
        {
            var backingField = new FieldNode(
                $"<{property.Name}>k__BackingField",
                property.PropertyType,
                FieldAttributes.Private);

            MethodNode getMethod = null;
            MethodNode setMethod = null;

            if (property.CanRead)
            {
                getMethod = new MethodNode(
                    property.GetMethod,
                    implementationMethod: null,
                    property.GetMethod!.Name,
                    MethodBuilderConstants.InterfaceMethodAttributes,
                    new BackingFieldGetBody(backingField.Name),
                    parameters: ParameterNodeFactory.FromMethod(property.GetMethod),
                    genericParameters: GenericParameterNodeFactory.FromMethod(property.GetMethod),
                    attributes: AttributeNodeFactory.FromCustomAttributes(property.GetMethod.CustomAttributes),
                    overridesMethod: property.GetMethod,
                    predicateMethod: property.GetMethod);
            }

            if (property.CanWrite)
            {
                setMethod = new MethodNode(
                    property.SetMethod,
                    implementationMethod: null,
                    property.SetMethod!.Name,
                    MethodBuilderConstants.InterfaceMethodAttributes,
                    new BackingFieldSetBody(backingField.Name),
                    parameters: ParameterNodeFactory.FromMethod(property.SetMethod),
                    genericParameters: GenericParameterNodeFactory.FromMethod(property.SetMethod),
                    attributes: AttributeNodeFactory.FromCustomAttributes(property.SetMethod.CustomAttributes),
                    overridesMethod: property.SetMethod,
                    predicateMethod: property.SetMethod);
            }

            var attrs = AttributeNodeFactory.FromCustomAttributes(property.CustomAttributes);

            var isPartial = (property.GetMethod?.IsPartialMethod() ?? false)
                || (property.SetMethod?.IsPartialMethod() ?? false);

            return new PropertyNode(
                property.Name,
                property.PropertyType,
                property.Attributes,
                attrs,
                getMethod,
                setMethod,
                backingField,
                isPartial: isPartial);
        }

        private ProxyTypeNode BuildProxyType(Type[] interfaceTypes)
        {
            return BuildProxyTypeCore(_proxyName, null, interfaceTypes);
        }

        private ProxyTypeNode BuildProxyTypeWithStub(string proxyName, Type stubImplType, Type[] interfaceTypes)
        {
            return BuildProxyTypeCore(proxyName, stubImplType, interfaceTypes);
        }

        private ProxyTypeNode BuildProxyTypeCore(string proxyName, Type stubImplType, Type[] interfaceTypes)
        {
            var genericParams = GenericParameterNodeFactory.FromType(_interfaceType);

            var attributes = new List<AttributeNode>
            {
                AttributeNodeFactory.CreateMarker(typeof(NonAspectAttribute)),
                AttributeNodeFactory.CreateMarker(typeof(DynamicallyAttribute))
            };

            var fields = new List<FieldNode>
            {
                new FieldNode("_activatorFactory", typeof(IAspectActivatorFactory), FieldAttributes.Private),
                new FieldNode("_implementation", _interfaceType, FieldAttributes.Private)
            };

            var constructor = new ConstructorNode(
                ConstructorKind.InterfaceProxyCtorWithFactory,
                MethodAttributes.Public,
                MethodUtils.ObjectCtor.CallingConvention,
                new[] { typeof(IAspectActivatorFactory) },
                baseConstructor: null,
                parameters: null,
                attributes: null,
                new[]
                {
                    new FieldAssignmentNode("_activatorFactory", 1)
                },
                stubImplType != null ? new TargetCreationNode(stubImplType, "_implementation") : null);

            var methods = new List<MethodNode>();
            var properties = new List<PropertyNode>();
            var methodConstants = new List<MethodConstantNode>();

            BuildInterfaceProxyMembers(
                _interfaceType, _additionalInterfaces, stubImplType, _aspectValidator,
                methods, properties, methodConstants);

            return new ProxyTypeNode(
                proxyName,
                ProxyKind.InterfaceProxy,
                _interfaceType,
                typeof(object),
                interfaceTypes,
                genericParams,
                attributes,
                fields,
                new[] { constructor },
                methods,
                properties,
                methodConstants);
        }

        internal static void BuildInterfaceProxyMembers(
            Type interfaceType,
            Type[] additionalInterfaces,
            Type implType,
            IAspectValidator aspectValidator,
            List<MethodNode> methods,
            List<PropertyNode> properties,
            List<MethodConstantNode> methodConstants)
        {
            var resolvedImplType = implType ?? interfaceType;
            var covariantReturnMethods = resolvedImplType.GetCovariantReturnMethods();

            // Primary interface methods
            foreach (var method in interfaceType.GetTypeInfo().DeclaredMethods.Where(x => !x.IsPropertyBinding()))
            {
                var covariantReturnMethod = FindCovariantReturnMethod(method);
                var implMethod = covariantReturnMethod ?? ResolveImplementationMethod(method, resolvedImplType);
                var body = MethodBodyFactory.DecideBody(method, implMethod, method, aspectValidator, interfaceType);
                var node = BuildProxyMethod(method, implMethod, method.Name,
                    MethodBuilderConstants.InterfaceMethodAttributes, body, method, method, methodConstants);
                methods.Add(node);
            }

            // Additional interface methods (explicit)
            foreach (var iface in additionalInterfaces)
            {
                foreach (var method in iface.GetTypeInfo().DeclaredMethods.Where(x => !x.IsPropertyBinding()))
                {
                    var covariantReturnMethod = FindCovariantReturnMethod(method);
                    var implMethod = covariantReturnMethod ?? ResolveImplementationMethod(method, resolvedImplType);
                    var body = MethodBodyFactory.DecideBody(method, implMethod, method, aspectValidator, interfaceType);
                    var node = BuildProxyMethod(method, implMethod, method.GetName(),
                        MethodBuilderConstants.ExplicitMethodAttributes, body, method, method, methodConstants);
                    methods.Add(node);
                }
            }

            // Primary interface properties
            foreach (var property in interfaceType.GetTypeInfo().DeclaredProperties)
            {
                var covariantReturnGetter = FindCovariantReturnGetter(property);
                properties.Add(BuildProxyProperty(property, property.Name, resolvedImplType, aspectValidator,
                    interfaceType, MethodBuilderConstants.InterfaceMethodAttributes, methods, methodConstants, covariantReturnGetter));
            }

            // Additional interface properties (explicit)
            foreach (var iface in additionalInterfaces)
            {
                foreach (var property in iface.GetTypeInfo().DeclaredProperties)
                {
                    var covariantReturnGetter = FindCovariantReturnGetter(property);
                    properties.Add(BuildProxyProperty(property, property.GetDisplayName(), resolvedImplType, aspectValidator,
                        interfaceType, MethodBuilderConstants.ExplicitMethodAttributes, methods, methodConstants, covariantReturnGetter));
                }
            }

            MethodInfo FindCovariantReturnMethod(MethodInfo interfaceMethod)
            {
                return covariantReturnMethods
                    .Where(m => m.InterfaceDeclarations.Contains(interfaceMethod))
                    .OrderByDescending(m => m.InheritanceDepth) // find most concrete covariant return method
                    .FirstOrDefault()
                    .CovariantReturnMethod;
            }

            MethodInfo FindCovariantReturnGetter(PropertyInfo property)
            {
                if (property.CanRead == false || property.CanWrite)
                    return null;

                return covariantReturnMethods
                    .Where(m => m.InterfaceDeclarations.Contains(property.GetMethod))
                    .OrderByDescending(m => m.InheritanceDepth) // find most concrete covariant return method
                    .FirstOrDefault()
                    .CovariantReturnMethod;
            }
        }

        private static PropertyNode BuildProxyProperty(
            PropertyInfo property,
            string name,
            Type implType,
            IAspectValidator aspectValidator,
            Type serviceType,
            MethodAttributes methodAttrs,
            List<MethodNode> methods,
            List<MethodConstantNode> methodConstants,
            MethodInfo covariantReturnGetter)
        {
            MethodNode getMethod = null;
            MethodNode setMethod = null;

            if (property.CanRead)
            {
                var method = property.GetMethod;
                var implMethod = covariantReturnGetter ?? ResolveImplementationMethod(method, implType);
                var body = MethodBodyFactory.DecideBody(method, implMethod, method, aspectValidator, serviceType);
                var overrides = methodAttrs == MethodBuilderConstants.ExplicitMethodAttributes ? method : method;
                getMethod = BuildProxyMethod(method, implMethod, methodAttrs == MethodBuilderConstants.ExplicitMethodAttributes ? method.GetName() : method.Name,
                    methodAttrs, body, method, method, methodConstants);
            }

            if (property.CanWrite)
            {
                var method = property.SetMethod;
                var implMethod = ResolveImplementationMethod(method, implType);
                var body = MethodBodyFactory.DecideBody(method, implMethod, method, aspectValidator, serviceType);
                setMethod = BuildProxyMethod(method, implMethod, methodAttrs == MethodBuilderConstants.ExplicitMethodAttributes ? method.GetName() : method.Name,
                    methodAttrs, body, method, method, methodConstants);
            }

            var attrs = new List<AttributeNode>
            {
                AttributeNodeFactory.CreateMarker(typeof(DynamicallyAttribute))
            };
            attrs.AddRange(AttributeNodeFactory.FromCustomAttributes(property.CustomAttributes));

            var isPartial = (property.GetMethod?.IsPartialMethod() ?? false)
                || (property.SetMethod?.IsPartialMethod() ?? false);

            return new PropertyNode(name, property.PropertyType, property.Attributes, attrs, getMethod, setMethod, isPartial: isPartial);
        }

        internal static MethodNode BuildProxyMethod(
            MethodInfo serviceMethod,
            MethodInfo implMethod,
            string name,
            MethodAttributes attributes,
            MethodBodyNode body,
            MethodInfo overridesMethod,
            MethodInfo predicateMethod,
            List<MethodConstantNode> methodConstants)
        {
            var genericParams = GenericParameterNodeFactory.FromMethod(serviceMethod);
            var parameters = ParameterNodeFactory.FromMethod(serviceMethod);

            var attrs = new List<AttributeNode>
            {
                AttributeNodeFactory.CreateMarker(typeof(DynamicallyAttribute))
            };
            attrs.AddRange(AttributeNodeFactory.FromCustomAttributes(serviceMethod.CustomAttributes));

            // Register method constants for aspect activator body
            if (body is AspectActivatorBody activatorBody && !activatorBody.IsGeneric)
            {
                var serviceKey = $"service{serviceMethod.GetDisplayName()}";
                var implKey = $"impl{implMethod.GetDisplayName()}";
                var proxyKey = $"proxy{serviceMethod.GetDisplayName()}";
                var predicateKey = $"predicate{predicateMethod.GetDisplayName()}";

                methodConstants.Add(new MethodConstantNode(serviceKey, serviceMethod));
                methodConstants.Add(new MethodConstantNode(implKey, implMethod));
                // proxyMethod will be the generated MethodBuilder - stored as null, resolved during visit
                methodConstants.Add(new MethodConstantNode(proxyKey, null));
                methodConstants.Add(new MethodConstantNode(predicateKey, predicateMethod));
            }

            return new MethodNode(
                serviceMethod,
                implMethod,
                name,
                attributes,
                body,
                parameters,
                genericParams,
                attrs,
                overridesMethod,
                predicateMethod);
        }

        internal static MethodInfo ResolveImplementationMethod(MethodInfo method, Type implType)
        {
            if (method.DeclaringType == implType)
            {
                return method;
            }

            if (method.DeclaringType != null
                && method.DeclaringType.GetTypeInfo().IsGenericType
                && implType.GetTypeInfo().IsGenericTypeDefinition
                && method.DeclaringType.GetGenericTypeDefinition() == implType)
            {
                return method;
            }

            var implementationMethod = implType.GetTypeInfo().GetMethodBySignature(method);
            if (implementationMethod != null)
                return implementationMethod;

            var interfaces = implType.GetInterfaces();
            if (interfaces == null || interfaces.Length == 0)
                throw new MissingMethodException($"Type '{implType}' does not contain a method '{method}'.");

            var abstractInterfaces = interfaces.Where(f => f.GetCustomAttribute(typeof(AbstractInterceptorAttribute)) != null).ToArray();
            var searchInterfaces = abstractInterfaces.Length > 0 ? abstractInterfaces : interfaces;

            foreach (var iface in searchInterfaces)
            {
                implementationMethod = iface.GetTypeInfo().GetMethodBySignature(method);
                if (implementationMethod != null) return implementationMethod;
            }

            throw new MissingMethodException($"Type '{implType}' does not contain a method '{method}'.");
        }
    }

    internal static class MethodBuilderConstants
    {
        internal const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        internal const MethodAttributes InterfaceMethodAttributes = MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        internal const MethodAttributes OverrideMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;
    }
}
