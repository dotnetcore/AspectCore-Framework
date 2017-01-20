using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Generator;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class AspectPropertyGenerator : PropertyGenerator
    {
        private readonly PropertyInfo propertyInfo;
        private readonly Type serviceType;
        private readonly Type parentType;
        private readonly FieldBuilder serviceInstanceFieldBuilder;
        private readonly FieldBuilder serviceProviderFieldBuilder;

        public AspectPropertyGenerator(TypeBuilder declaringMember, PropertyInfo propertyInfo,
             Type serviceType, Type parentType,
             FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder) :
             base(declaringMember)
        {
            this.propertyInfo = propertyInfo;
            this.serviceType = serviceType;
            this.parentType = parentType;
            this.serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            this.serviceProviderFieldBuilder = serviceProviderFieldBuilder;
        }

        public override CallingConventions CallingConventions
        {
            get
            {
                return CallingConventions.HasThis;
            }
        }

        public override bool CanRead
        {
            get
            {
                return propertyInfo.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return propertyInfo.CanWrite;
            }
        }

        public override MethodInfo GetMethod
        {
            get
            {
                return propertyInfo.GetGetMethod();
            }
        }

        public override PropertyAttributes PropertyAttributes
        {
            get
            {
                return propertyInfo.Attributes;
            }
        }

        public override string PropertyName
        {
            get
            {
                return propertyInfo.GetFullName();
            }
        }

        public override Type PropertyType
        {
            get
            {
                return propertyInfo.PropertyType;
            }
        }

        public override MethodInfo SetMethod
        {
            get
            {
                return propertyInfo.GetSetMethod();
            }
        }

        protected override MethodGenerator GetReadMethodGenerator(TypeBuilder declaringType)
        {
            return new AspectMethodGenerator(declaringType, serviceType, parentType, GetMethod, serviceInstanceFieldBuilder, serviceProviderFieldBuilder);
        }

        protected override MethodGenerator GetWriteMethodGenerator(TypeBuilder declaringType)
        {
            return new AspectMethodGenerator(declaringType, serviceType, parentType, SetMethod, serviceInstanceFieldBuilder, serviceProviderFieldBuilder);
        }
    }
}
