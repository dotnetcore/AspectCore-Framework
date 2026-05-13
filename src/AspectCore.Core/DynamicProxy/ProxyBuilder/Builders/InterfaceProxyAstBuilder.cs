using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Utils;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;

namespace AspectCore.DynamicProxy.ProxyBuilder.Builders
{
    internal class InterfaceProxyBuilder : IProxyTypeBuilder
    {
        private readonly string _name;
        private readonly Type _interfaceType;
        private readonly Type _implType;
        private readonly Type[] _additionalInterfaces;
        private readonly IAspectValidator _aspectValidator;

        public InterfaceProxyBuilder(
            string name,
            Type interfaceType,
            Type implType,
            Type[] additionalInterfaces,
            IAspectValidator aspectValidator)
        {
            _name = name;
            _interfaceType = interfaceType;
            _implType = implType;
            _additionalInterfaces = additionalInterfaces;
            _aspectValidator = aspectValidator;
        }

        public ProxyTypeNode[] Build()
        {
            var interfaces = new[] { _interfaceType }.Concat(_additionalInterfaces).Distinct().ToArray();
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
                ConstructorKind.InterfaceProxyCtorWithFactoryAndTarget,
                MethodAttributes.Public,
                MethodUtils.ObjectCtor.CallingConvention,
                new[] { typeof(IAspectActivatorFactory), _interfaceType },
                baseConstructor: null,
                parameters: null,
                attributes: null,
                new[]
                {
                    new FieldAssignmentNode("_activatorFactory", 1),
                    new FieldAssignmentNode("_implementation", 2)
                },
                targetCreation: null);

            var methods = new List<MethodNode>();
            var properties = new List<PropertyNode>();
            var methodConstants = new List<MethodConstantNode>();

            InterfaceImplBuilder.BuildInterfaceProxyMembers(
                _interfaceType, _additionalInterfaces, _implType, _aspectValidator,
                methods, properties, methodConstants);

            return new[]
            {
                new ProxyTypeNode(
                    _name,
                    ProxyKind.InterfaceProxy,
                    _interfaceType,
                    typeof(object),
                    interfaces,
                    genericParams,
                    attributes,
                    fields,
                    new[] { constructor },
                    methods,
                    properties,
                    methodConstants)
            };
        }
    }
}
