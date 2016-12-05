using AspectCore.Lite.Abstractions.Generator;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    internal class PrivateFieldGenerator : FieldGenerator
    {
        public PrivateFieldGenerator(string name, Type fieldType, TypeBuilder declaringMember) : base(declaringMember)
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
