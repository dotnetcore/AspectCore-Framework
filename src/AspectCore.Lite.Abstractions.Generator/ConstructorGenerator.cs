using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class ConstructorGenerator : MetaDataGenerator<TypeBuilder, ConstructorBuilder>
    { 
        public abstract MethodAttributes MethodAttributes { get; }

        public abstract CallingConventions CallingConventions { get; }

        public abstract Type[] ParameterTypes { get; }

        public ConstructorGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override ConstructorBuilder Accept(GeneratorVisitor visitor)
        {
            var constructorBuilder = DeclaringMember.DefineConstructor(MethodAttributes, CallingConventions, ParameterTypes);

            var ilGenerator = constructorBuilder.GetILGenerator();

            GeneratingConstructorBody(ilGenerator);

            return constructorBuilder;
        }

        protected abstract void GeneratingConstructorBody(ILGenerator ilGenerator);
    }
}
