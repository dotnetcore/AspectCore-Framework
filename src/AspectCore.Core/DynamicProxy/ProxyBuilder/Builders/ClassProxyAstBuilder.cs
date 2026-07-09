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
    internal class ClassProxyBuilder : IProxyTypeBuilder
    {
        private static readonly HashSet<string> Ignores = new HashSet<string> { "Finalize" };

        private readonly string _name;
        private readonly Type _serviceType;
        private readonly Type _implType;
        private readonly Type[] _additionalInterfaces;
        private readonly IAspectValidator _aspectValidator;

        public ClassProxyBuilder(
            string name,
            Type serviceType,
            Type implType,
            Type[] additionalInterfaces,
            IAspectValidator aspectValidator)
        {
            _name = name;
            _serviceType = serviceType;
            _implType = implType;
            _additionalInterfaces = additionalInterfaces;
            _aspectValidator = aspectValidator;
        }

        public ProxyTypeNode[] Build()
        {
            var interfaces = _additionalInterfaces.Distinct().ToArray();
            var genericParams = GenericParameterNodeFactory.FromType(_serviceType);

            var attributes = new List<AttributeNode>
            {
                AttributeNodeFactory.CreateMarker(typeof(NonAspectAttribute)),
                AttributeNodeFactory.CreateMarker(typeof(DynamicallyAttribute))
            };

            // Inherit impl type's attributes
            attributes.AddRange(AttributeNodeFactory.FromCustomAttributes(_implType.CustomAttributes));

            var fields = new List<FieldNode>
            {
                new FieldNode("_activatorFactory", typeof(IAspectActivatorFactory), FieldAttributes.Private),
                new FieldNode("_implementation", _serviceType, FieldAttributes.Private)
            };

            var constructors = BuildConstructors();
            var methods = new List<MethodNode>();
            var properties = new List<PropertyNode>();
            var methodConstants = new List<MethodConstantNode>();

            BuildClassMethods(methods, methodConstants);
            BuildClassProperties(properties, methods, methodConstants);
            BuildAdditionalInterfaceMembers(methods, properties, methodConstants);

            return new[]
            {
                new ProxyTypeNode(
                    _name,
                    ProxyKind.ClassProxy,
                    _serviceType,
                    _implType,
                    interfaces,
                    genericParams,
                    attributes,
                    fields,
                    constructors,
                    methods,
                    properties,
                    methodConstants)
            };
        }

        private List<ConstructorNode> BuildConstructors()
        {
            var constructors = _implType.GetTypeInfo().DeclaredConstructors
                .Where(c => !c.IsStatic && (c.IsPublic || c.IsFamily || c.IsFamilyAndAssembly || c.IsFamilyOrAssembly))
                .ToArray();

            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(
                    $"A suitable constructor for type {_serviceType.FullName} could not be located. Ensure the type is concrete and services are registered for all parameters of a public constructor.");
            }

            var result = new List<ConstructorNode>(constructors.Length);
            foreach (var ctor in constructors)
            {
                var parameterTypes = ctor.GetParameters().Select(p => p.ParameterType).ToArray();
                var allParamTypes = new[] { typeof(IAspectActivatorFactory) }.Concat(parameterTypes).ToArray();

                var ctorAttributes = new List<AttributeNode>
                {
                    AttributeNodeFactory.CreateMarker(typeof(DynamicallyAttribute))
                };
                ctorAttributes.AddRange(AttributeNodeFactory.FromCustomAttributes(ctor.CustomAttributes));

                var parameters = ParameterNodeFactory.FromConstructor(ctor);

                result.Add(new ConstructorNode(
                    ConstructorKind.ClassProxyCtorFromBase,
                    ctor.Attributes,
                    ctor.CallingConvention,
                    allParamTypes,
                    ctor,
                    parameters,
                    ctorAttributes,
                    new[]
                    {
                        new FieldAssignmentNode("_activatorFactory", 1)
                    },
                    targetCreation: null));
            }
            return result;
        }

        private MethodNode CreateClassProxyMethodNode(MethodInfo method, MethodInfo implMethod, MethodInfo overridesMethod, List<MethodConstantNode> methodConstants)
        {
            var body = MethodBodyFactory.DecideBody(method, implMethod, _aspectValidator, _serviceType);

            var attributes = MethodBuilderConstants.OverrideMethodAttributes;
            if (method.Attributes.HasFlag(MethodAttributes.Public))
                attributes |= MethodAttributes.Public;
            if (method.Attributes.HasFlag(MethodAttributes.Family))
                attributes |= MethodAttributes.Family;
            if (method.Attributes.HasFlag(MethodAttributes.FamORAssem))
                attributes |= MethodAttributes.FamORAssem;

            if (implMethod.IsCovariantReturnMethod())
                attributes |= MethodAttributes.NewSlot;

            var node = InterfaceImplBuilder.BuildProxyMethod(
                serviceMethod: method,
                implMethod: implMethod,
                name: method.Name,
                attributes: attributes,
                body: body,
                overridesMethod: overridesMethod,
                methodConstants: methodConstants);

            return node;
        }

        private void BuildClassMethods(List<MethodNode> methods, List<MethodConstantNode> methodConstants)
        {
            var covariantReturnMethods = _implType.GetCovariantReturnMethods();

            foreach (var method in _serviceType.GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => !x.IsPropertyBinding()))
            {
                if (!method.IsVisibleAndVirtual() || Ignores.Contains(method.Name))
                    continue;

                var (covariantReturnMethod, skip) = FindCovariantReturnMethod(method);
                if (skip)
                    continue;

                var (serviceMethod, implMethod) = covariantReturnMethod is null
                    ? (method, InterfaceImplBuilder.ResolveImplementationMethod(method, _implType))
                    : (covariantReturnMethod, covariantReturnMethod);

                var node = CreateClassProxyMethodNode(serviceMethod, implMethod, null, methodConstants);
                methods.Add(node);
            }

            (MethodInfo, bool Skip) FindCovariantReturnMethod(MethodInfo method)
            {
                var covariantReturn = covariantReturnMethods
                    .Where(m => method.IsOverriddenByCovariantReturnMethod(m.CovariantReturnMethod))
                    .OrderByDescending(m => m.InheritanceDepth) // find most concrete covariant return method
                    .FirstOrDefault();

                var overridden = covariantReturn.OverriddenMethod;
                if (overridden == null)
                    return (null, false);

                if (method.DeclaringType == method.ReflectedType)
                {
                    // the 'method' is declared in the _serviceType, and it is overridden by a covariant-return method in the _implType.
                    // In this case, use CovariantReturnMethod for both serviceMethod and implMethod.
                    return (covariantReturn.CovariantReturnMethod, false);
                }
                else
                {
                    // the 'method' is declared in a base class of the _serviceType, and it is overridden by a covariant-return method in the _implType.
                    // In this case, the covariant-return method will be visited in a later iteration when the base class is processed, so skip the current method to avoid conflicts.
                    return (null, true);
                }
            }
        }

        private void BuildClassProperties(List<PropertyNode> properties, List<MethodNode> methods, List<MethodConstantNode> methodConstants)
        {
            var covariantReturnMethods = _implType.GetCovariantReturnMethods();

            foreach (var property in _serviceType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!property.IsVisibleAndVirtual())
                    continue;

                MethodNode getMethod = null;
                MethodNode setMethod = null;

                if (property.CanRead)
                {
                    var (covariantReturnGetter, skip) = FindCovariantReturnPropertyGetter(property);
                    if (skip)
                        continue;

                    var method = property.GetMethod;
                    var (serviceMethod, implMethod) = covariantReturnGetter is null
                        ? (method, InterfaceImplBuilder.ResolveImplementationMethod(method, _implType))
                        : (covariantReturnGetter, covariantReturnGetter);

                    // when property.GetMethod is a covariant return type method, we need to define an override for it.
                    // otherwise, the typeBuilder.CreateTypeInfo() will run forever.
                    var overrides = property.GetMethod.IsInCovariantReturnChain()
                        ? property.GetMethod
                        : null;

                    getMethod = CreateClassProxyMethodNode(serviceMethod, implMethod, overrides, methodConstants);
                }

                if (property.CanWrite)
                {
                    var method = property.SetMethod;
                    var implMethod = InterfaceImplBuilder.ResolveImplementationMethod(method, _implType);
                    setMethod = CreateClassProxyMethodNode(method, implMethod, null, methodConstants);
                }

                var attrs = new List<AttributeNode>
                {
                    AttributeNodeFactory.CreateMarker(typeof(DynamicallyAttribute))
                };
                attrs.AddRange(AttributeNodeFactory.FromCustomAttributes(property.CustomAttributes));

                properties.Add(new PropertyNode(property.Name, property.PropertyType, property.Attributes, attrs, getMethod, setMethod));
            }

            (MethodInfo, bool Skip) FindCovariantReturnPropertyGetter(PropertyInfo property)
            {
                if (property.GetMethod is not { } getter)
                    return (null, false);

                // covariant return type is not supported for set method, so skip it.
                if (property.CanWrite)
                    return (null, false);

                // property is inherited from base class, and the getter is overridden by a covariant return type method in the implType.
                // in this case, we need to skip the property to avoid conflicts when building the proxy type.
                if (property.PropertyType != getter.ReturnType
                   && getter.IsInCovariantReturnChain())
                    return (null, true);

                var covariantReturn = covariantReturnMethods
                    .Where(m => getter.IsOverriddenByCovariantReturnMethod(m.CovariantReturnMethod))
                    .OrderByDescending(m => m.InheritanceDepth) // find most concrete covariant return method
                    .FirstOrDefault();

                var overridden = covariantReturn.OverriddenMethod;
                if (overridden == null)
                    return (null, false);

                if (getter.DeclaringType == getter.ReflectedType)
                {
                    // the 'getter' is declared in the _serviceType, and it is overridden by a covariant-return method in the _implType.
                    // In this case, use CovariantReturnMethod for both serviceMethod and implMethod.
                    return (covariantReturn.CovariantReturnMethod, false);
                }
                else
                {
                    // the 'getter' is declared in a base class of the _serviceType, and it is overridden by a covariant-return method in the _implType.
                    // In this case, the covariant-return method will be visited in a later iteration when the base class is processed, so skip the current method to avoid conflicts.
                    return (null, true);
                }
            }
        }

        private void BuildAdditionalInterfaceMembers(List<MethodNode> methods, List<PropertyNode> properties, List<MethodConstantNode> methodConstants)
        {
            foreach (var iface in _additionalInterfaces)
            {
                foreach (var method in iface.GetTypeInfo().DeclaredMethods.Where(x => !x.IsPropertyBinding()))
                {
                    var implMethod = InterfaceImplBuilder.ResolveImplementationMethod(method, _implType);
                    var body = MethodBodyFactory.DecideBody(method, implMethod, _aspectValidator, _serviceType);
                    var node = InterfaceImplBuilder.BuildProxyMethod(
                        method, implMethod, method.GetName(),
                        MethodBuilderConstants.ExplicitMethodAttributes, body, method, methodConstants);
                    methods.Add(node);
                }

                foreach (var property in iface.GetTypeInfo().DeclaredProperties)
                {
                    MethodNode getMethod = null;
                    MethodNode setMethod = null;

                    if (property.CanRead)
                    {
                        var method = property.GetMethod;
                        var implMethod = InterfaceImplBuilder.ResolveImplementationMethod(method, _implType);
                        var body = MethodBodyFactory.DecideBody(method, implMethod, _aspectValidator, _serviceType);
                        getMethod = InterfaceImplBuilder.BuildProxyMethod(
                            method, implMethod, method.GetName(),
                            MethodBuilderConstants.ExplicitMethodAttributes, body, method, methodConstants);
                    }

                    if (property.CanWrite)
                    {
                        var method = property.SetMethod;
                        var implMethod = InterfaceImplBuilder.ResolveImplementationMethod(method, _implType);
                        var body = MethodBodyFactory.DecideBody(method, implMethod, _aspectValidator, _serviceType);
                        setMethod = InterfaceImplBuilder.BuildProxyMethod(
                            method, implMethod, method.GetName(),
                            MethodBuilderConstants.ExplicitMethodAttributes, body, method, methodConstants);
                    }

                    var attrs = new List<AttributeNode>
                    {
                        AttributeNodeFactory.CreateMarker(typeof(DynamicallyAttribute))
                    };
                    attrs.AddRange(AttributeNodeFactory.FromCustomAttributes(property.CustomAttributes));

                    properties.Add(new PropertyNode(property.GetDisplayName(), property.PropertyType, property.Attributes, attrs, getMethod, setMethod));
                }
            }
        }
    }
}
