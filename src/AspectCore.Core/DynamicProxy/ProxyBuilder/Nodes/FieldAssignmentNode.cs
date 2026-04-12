using System;

namespace AspectCore.DynamicProxy.ProxyBuilder.Nodes
{
    internal class FieldAssignmentNode
    {
        public string FieldName { get; }

        public int SourceArgIndex { get; }

        public FieldAssignmentNode(string fieldName, int sourceArgIndex)
        {
            FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            SourceArgIndex = sourceArgIndex;
        }
    }
}
