using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Core.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal sealed class MethodAttributeGenerator : AttributeGenerator<MethodBuilder>
    {
        private readonly CustomAttributeData _customAttributeData;

        public MethodAttributeGenerator(MethodBuilder declaringMember, CustomAttributeData customAttributeData) : base(declaringMember)
        {
            _customAttributeData = customAttributeData;
        }

        public override CustomAttributeData CustomAttributeData
        {
            get
            {
                return _customAttributeData;
            }
        }
    }
}