using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal class ProxyFieldGnerator : FieldGenerator
    {
        public ProxyFieldGnerator(string name, Type fieldType, TypeBuilder declaringMember) : base(declaringMember)
        {
            FieldName = name;
            FieldType = fieldType;
        }

        public override FieldAttributes FieldAttributes
        {
            get
            {
                return FieldAttributes.Private;
            }
        }

        public override string FieldName { get; }

        public override Type FieldType { get; }
    }
}
