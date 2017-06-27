using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Core.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal sealed class MethodAttributeGenerator : AttributeGenerator<MethodBuilder>
    {
        private readonly Type _attributeType;

        public MethodAttributeGenerator(MethodBuilder declaringMember, Type attributeType) : base(declaringMember)
        {
            _attributeType = attributeType;
        }

        public MethodAttributeGenerator(MethodBuilder declaringMember, CustomAttributeData customAttributeData) : base(declaringMember)
        {
            CustomAttributeData = customAttributeData;
        }

        public override CustomAttributeData CustomAttributeData { get; }

        protected override CustomAttributeBuilder ExecuteBuild()
        {
            if (CustomAttributeData != null)
            {
                var builder = base.ExecuteBuild();
                DeclaringMember.SetCustomAttribute(builder);
                return builder;
            }
            else
            {
                var builder = new CustomAttributeBuilder(_attributeType.GetTypeInfo().GetConstructor(Type.EmptyTypes), EmptyArray<object>.Value);
                DeclaringMember.SetCustomAttribute(builder);
                return builder;
            }
        }
    }
}