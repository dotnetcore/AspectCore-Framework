using System;
using System.Reflection.Emit;
using AspectCore.Abstractions;

namespace AspectCore.Core.Generator
{
    [NonAspect]
    public abstract class MethodBodyGenerator : AbstractGenerator<MethodBuilder, ILGenerator>
    {
        protected MethodBodyGenerator(MethodBuilder declaringMember) : base(declaringMember)
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
