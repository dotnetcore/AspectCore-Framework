using AspectCore.Lite.Abstractions.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    internal class PrivateFieldGenerator : FieldGenerator
    {
        public PrivateFieldGenerator(string name, Type fieldType, TypeBuilder declaringMember) : base(declaringMember)
        {

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
