using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core.Generator
{
    [NonAspect]
    public abstract class AttributeGenerator<TDeclaringMember> : AbstractGenerator<TDeclaringMember, CustomAttributeBuilder>
    {
        public abstract CustomAttributeData CustomAttributeData { get; }

        public AttributeGenerator(TDeclaringMember declaringMember) : base(declaringMember)
        {
        }

        protected override CustomAttributeBuilder ExecuteBuild()
        {
            return new CustomAttributeBuilderProvider(CustomAttributeData).CustomAttributeBuilder;
        }
    }
}