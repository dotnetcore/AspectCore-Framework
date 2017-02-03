using AspectCore.Abstractions.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class AccessorPropertyGenerator : PropertyGenerator
    {
        private readonly FieldBuilder fieldBuilder;

        public AccessorPropertyGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        public override CallingConventions CallingConventions { get; } = CallingConventions.HasThis;

        public override PropertyAttributes PropertyAttributes { get; } = PropertyAttributes.None;

        public override MethodInfo SetMethod
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanRead { get; } = true;

        public override bool CanWrite { get; } = false;

        public override MethodInfo GetMethod
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public override string PropertyName { get; }

        public override Type PropertyType { get; }

     

        protected override MethodGenerator GetReadMethodGenerator(TypeBuilder declaringType)
        {
            throw new NotImplementedException();
        }

        protected override MethodGenerator GetWriteMethodGenerator(TypeBuilder declaringType)
        {
            throw new NotImplementedException();
        }
    }
}
