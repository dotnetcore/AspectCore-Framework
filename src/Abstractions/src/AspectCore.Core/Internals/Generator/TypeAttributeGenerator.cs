using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Core.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal sealed class TypeAttributeGenerator : AttributeGenerator<TypeBuilder>
    {
        private readonly CustomAttributeData _customAttributeData;
        public TypeAttributeGenerator(TypeBuilder declaringMember, CustomAttributeData customAttributeData) : base(declaringMember)
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