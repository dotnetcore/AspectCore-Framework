using System;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Generator
{
    public abstract class MethodBodyGenerator : AbstractGenerator<MethodBuilder, ILGenerator>
    {
        public MethodBodyGenerator(MethodBuilder declaringMember) : base(declaringMember)
        {
        }

        protected override ILGenerator ExecuteBuild()
        {
            var ilGenerator = DeclaringMember.GetILGenerator();
            GeneratingMethodBody(ilGenerator);
            return ilGenerator;
        }

        protected abstract void GeneratingMethodBody(ILGenerator ilGenerator);
    }
}
