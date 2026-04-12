using System;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class TargetCreationNode
    {
        public Type ImplType { get; }

        public string TargetFieldName { get; }

        public TargetCreationNode(Type implType, string targetFieldName)
        {
            ImplType = implType ?? throw new ArgumentNullException(nameof(implType));
            TargetFieldName = targetFieldName ?? throw new ArgumentNullException(nameof(targetFieldName));
        }
    }
}
