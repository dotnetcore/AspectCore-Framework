using System;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;

namespace AspectCore.DynamicProxy.ProxyBuilder
{
    internal interface IProxyBuilderVisitor
    {
        Type VisitProxyType(ProxyTypeNode node);

        void VisitField(FieldNode node);

        void VisitConstructor(ConstructorNode node);

        void VisitMethod(MethodNode node);

        void VisitProperty(PropertyNode node);

        void VisitParameter(ParameterNode node);

        void VisitGenericParameter(GenericParameterNode node);

        void VisitAttribute(AttributeNode node);

        void VisitMethodConstant(MethodConstantNode node);

        void VisitDirectDelegationBody(DirectDelegationBody node);

        void VisitReflectorDelegationBody(ReflectorDelegationBody node);

        void VisitRecordCloneBody(RecordCloneBody node);

        void VisitAspectActivatorBody(AspectActivatorBody node);

        void VisitStubBody(StubBody node);

        void VisitBackingFieldGetBody(BackingFieldGetBody node);

        void VisitBackingFieldSetBody(BackingFieldSetBody node);
    }
}
