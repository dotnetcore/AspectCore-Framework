using System.Reflection.Emit;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class MethodBodyGenerator : Generator<MethodBuilder, ILGenerator>
    {
        public MethodBodyGenerator(MethodBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override ILGenerator Accept(GeneratorVisitor visitor)
        {
            var ilGenerator = DeclaringMember.GetILGenerator();
            GeneratingMethodBody(ilGenerator);
            return ilGenerator;
        }

        protected abstract void GeneratingMethodBody(ILGenerator ilGenerator);
    }
}
