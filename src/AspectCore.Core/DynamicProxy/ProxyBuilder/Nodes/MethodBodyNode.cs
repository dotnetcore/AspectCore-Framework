using System;
using System.Reflection;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal abstract class MethodBodyNode
    {
        public abstract void Accept(IProxyBuilderVisitor visitor);
    }

    internal class DirectDelegationBody : MethodBodyNode
    {
        public string TargetFieldName { get; }

        public MethodInfo TargetMethod { get; }

        public MethodInfo ServiceMethod { get; }

        public bool IsCallvirt { get; }

        public Type ServiceType { get; }

        public DirectDelegationBody(string targetFieldName, MethodInfo targetMethod, MethodInfo serviceMethod, bool isCallvirt, Type serviceType)
        {
            TargetFieldName = targetFieldName ?? throw new ArgumentNullException(nameof(targetFieldName));
            TargetMethod = targetMethod ?? throw new ArgumentNullException(nameof(targetMethod));
            ServiceMethod = serviceMethod ?? throw new ArgumentNullException(nameof(serviceMethod));
            IsCallvirt = isCallvirt;
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitDirectDelegationBody(this);
    }

    internal class ReflectorDelegationBody : MethodBodyNode
    {
        public MethodInfo ImplementationMethod { get; }

        public MethodInfo ServiceMethod { get; }

        public ReflectorDelegationBody(MethodInfo implementationMethod, MethodInfo serviceMethod)
        {
            ImplementationMethod = implementationMethod ?? throw new ArgumentNullException(nameof(implementationMethod));
            ServiceMethod = serviceMethod ?? throw new ArgumentNullException(nameof(serviceMethod));
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitReflectorDelegationBody(this);
    }

    internal class AspectActivatorBody : MethodBodyNode
    {
        public MethodInfo ServiceMethod { get; }

        public MethodInfo ImplementationMethod { get; }

        /// <summary>
        /// Gets the method used to evaluate configured <see cref="AspectCore.Configuration.AspectPredicate"/> filters.
        /// </summary>
        public MethodInfo PredicateMethod { get; }

        public bool IsGeneric { get; }

        public ReturnKind ReturnKind { get; }

        public Type ReturnType { get; }

        public AspectActivatorBody(MethodInfo serviceMethod, MethodInfo implementationMethod, MethodInfo predicateMethod, bool isGeneric, ReturnKind returnKind, Type returnType)
        {
            ServiceMethod = serviceMethod ?? throw new ArgumentNullException(nameof(serviceMethod));
            ImplementationMethod = implementationMethod ?? throw new ArgumentNullException(nameof(implementationMethod));
            PredicateMethod = predicateMethod ?? throw new ArgumentNullException(nameof(predicateMethod));
            IsGeneric = isGeneric;
            ReturnKind = returnKind;
            ReturnType = returnType;
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitAspectActivatorBody(this);
    }

    internal class StubBody : MethodBodyNode
    {
        public Type ReturnType { get; }

        public StubBody(Type returnType)
        {
            ReturnType = returnType;
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitStubBody(this);
    }

    internal class BackingFieldGetBody : MethodBodyNode
    {
        public string FieldName { get; }

        public BackingFieldGetBody(string fieldName)
        {
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitBackingFieldGetBody(this);
    }

    internal class BackingFieldSetBody : MethodBodyNode
    {
        public string FieldName { get; }

        public BackingFieldSetBody(string fieldName)
        {
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
        }

        public override void Accept(IProxyBuilderVisitor visitor) => visitor.VisitBackingFieldSetBody(this);
    }
}
