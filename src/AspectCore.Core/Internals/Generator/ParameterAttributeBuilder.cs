using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using AspectCore.Core.Generator;
using AspectCore.Core.Internal;

namespace AspectCore.Core.Internal.Generator
{
    internal sealed class ParameterAttributeBuilder : AttributeGenerator<ParameterBuilder>
    {
        private readonly Type _attributeType;

        public ParameterAttributeBuilder(ParameterBuilder declaringMember, Type attributeType) : base(declaringMember)
        {
            _attributeType = attributeType;
        }

        public ParameterAttributeBuilder(ParameterBuilder declaringMember, CustomAttributeData customAttributeData) : base(declaringMember)
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
