using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class MethodNode : ProxyBuilderNode
    {
        public MethodInfo ServiceMethod { get; }

        public MethodInfo ImplementationMethod { get; }

        public string Name { get; }

        public MethodAttributes MethodAttributes { get; }

        public MethodBodyNode Body { get; }

        public IReadOnlyList<ParameterNode> Parameters { get; }

        public IReadOnlyList<GenericParameterNode> GenericParameters { get; }

        public IReadOnlyList<AttributeNode> Attributes { get; }

        public MethodInfo OverridesMethod { get; }

        public MethodNode(
            MethodInfo serviceMethod,
            MethodInfo implementationMethod,
            string name,
            MethodAttributes methodAttributes,
            MethodBodyNode body,
            IReadOnlyList<ParameterNode> parameters,
            IReadOnlyList<GenericParameterNode> genericParameters,
            IReadOnlyList<AttributeNode> attributes,
            MethodInfo overridesMethod)
        {
            ServiceMethod = serviceMethod ?? throw new ArgumentNullException(nameof(serviceMethod));
            ImplementationMethod = implementationMethod;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            MethodAttributes = methodAttributes;
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Parameters = parameters ?? Array.Empty<ParameterNode>();
            GenericParameters = genericParameters ?? Array.Empty<GenericParameterNode>();
            Attributes = attributes ?? Array.Empty<AttributeNode>();
            OverridesMethod = overridesMethod;
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitMethod(this);
    }
}
