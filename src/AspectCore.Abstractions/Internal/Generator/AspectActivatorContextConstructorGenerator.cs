using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Generator;


namespace AspectCore.Abstractions.Internal.Generator
{
    internal class AspectActivatorContextConstructorGenerator : ConstructorGenerator
    {
        private readonly FieldGenerator[] _fields;
        public AspectActivatorContextConstructorGenerator(TypeBuilder declaringMember, params FieldGenerator[] fields) : base(declaringMember)
        {
            _fields = fields;
        }

        public override CallingConventions CallingConventions
        {
            get
            {
                return CallingConventions.HasThis;
            }
        }

        public override MethodAttributes MethodAttributes
        {
            get
            {
                return MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName;
            }
        }

        public override Type[] ParameterTypes
        {
            get
            {
                return new Type[] { typeof(Type), typeof(MethodInfo), typeof(MethodInfo), typeof(MethodInfo), typeof(object), typeof(object), typeof(object[]) };
            }
        }

        protected override void GeneratingConstructorBody(ILGenerator ilGenerator)
        {
            ilGenerator.EmitThis();
            var baseCtor = typeof(AspectActivatorContext).GetTypeInfo().DeclaredConstructors.First();
            ilGenerator.Emit(OpCodes.Call, baseCtor);
            for (int i = 0; i < _fields.Length; i++)
            {
                ilGenerator.EmitThis();
                ilGenerator.EmitLoadArg(i + 1);
                ilGenerator.Emit(OpCodes.Stfld, _fields[i].Build());
            }
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
