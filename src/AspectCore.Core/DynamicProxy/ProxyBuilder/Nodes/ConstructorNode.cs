using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class ConstructorNode : ProxyBuilderNode
    {
        public ConstructorKind Kind { get; }

        public ConstructorInfo BaseConstructor { get; }

        public MethodAttributes MethodAttributes { get; }

        public CallingConventions CallingConvention { get; }

        public IReadOnlyList<ParameterNode> Parameters { get; }

        public IReadOnlyList<AttributeNode> Attributes { get; }

        public IReadOnlyList<FieldAssignmentNode> FieldAssignments { get; }

        public TargetCreationNode TargetCreation { get; }

        public Type[] ParameterTypes { get; }

        public ConstructorNode(
            ConstructorKind kind,
            MethodAttributes methodAttributes,
            CallingConventions callingConvention,
            Type[] parameterTypes,
            ConstructorInfo baseConstructor,
            IReadOnlyList<ParameterNode> parameters,
            IReadOnlyList<AttributeNode> attributes,
            IReadOnlyList<FieldAssignmentNode> fieldAssignments,
            TargetCreationNode targetCreation)
        {
            Kind = kind;
            MethodAttributes = methodAttributes;
            CallingConvention = callingConvention;
            ParameterTypes = parameterTypes ?? Type.EmptyTypes;
            BaseConstructor = baseConstructor;
            Parameters = parameters ?? Array.Empty<ParameterNode>();
            Attributes = attributes ?? Array.Empty<AttributeNode>();
            FieldAssignments = fieldAssignments ?? Array.Empty<FieldAssignmentNode>();
            TargetCreation = targetCreation;
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitConstructor(this);
    }
}
