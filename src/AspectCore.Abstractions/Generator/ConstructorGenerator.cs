using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Generator
{
    public abstract class ConstructorGenerator : AbstractGenerator<TypeBuilder, ConstructorBuilder>
    { 
        public abstract MethodAttributes MethodAttributes { get; }

        public abstract CallingConventions CallingConventions { get; }

        public abstract Type[] ParameterTypes { get; }

        protected ConstructorGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override ConstructorBuilder ExecuteBuild()
        {
            var constructorBuilder = DeclaringMember.DefineConstructor(MethodAttributes, CallingConventions, ParameterTypes);

            var ilGenerator = constructorBuilder.GetILGenerator();

            GeneratingConstructorBody(ilGenerator);

            return constructorBuilder;
        }

        protected abstract void GeneratingConstructorBody(ILGenerator ilGenerator);
    }
}
