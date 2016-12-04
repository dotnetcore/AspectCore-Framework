using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.Generators
{
    public abstract class DefaultConstructorGenerator: ConstructorGenerator
    {
        public sealed override CallingConventions CallingConventions
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public sealed override Type[] ParameterTypes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DefaultConstructorGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override ConstructorBuilder Accept(GeneratorVisitor visitor)
        {
            var constructorBuilder = DeclaringMember.DefineDefaultConstructor(MethodAttributes);

            var ilGenerator = constructorBuilder.GetILGenerator();

            GeneratingConstructorBody(ilGenerator);

            return constructorBuilder;
        }
    }
}
