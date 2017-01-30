using AspectCore.Abstractions.Extensions;
using AspectCore.Abstractions.Generator;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Abstractions.Resolution.Generators
{
    internal class ProxyPropertyGenerator : PropertyGenerator
    {
        protected readonly PropertyInfo propertyInfo;
        protected readonly Type serviceType;
        protected readonly Type parentType;
        protected readonly FieldBuilder serviceInstanceFieldBuilder;
        protected readonly FieldBuilder serviceProviderFieldBuilder;
        protected readonly bool isImplementExplicitly;

        public ProxyPropertyGenerator(TypeBuilder declaringMember, PropertyInfo propertyInfo,
             Type serviceType, Type parentType,
             FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder,
             bool isImplementExplicitly) :
             base(declaringMember)
        {
            this.propertyInfo = propertyInfo;
            this.serviceType = serviceType;
            this.parentType = parentType;
            this.serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            this.serviceProviderFieldBuilder = serviceProviderFieldBuilder;
            this.isImplementExplicitly = isImplementExplicitly;
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
                return isImplementExplicitly ? propertyInfo.GetFullName() : propertyInfo.Name;
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
            return new ProxyMethodGenerator(declaringType, serviceType, parentType, GetMethod, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, isImplementExplicitly);
        }

        protected override MethodGenerator GetWriteMethodGenerator(TypeBuilder declaringType)
        {
            return new ProxyMethodGenerator(declaringType, serviceType, parentType, SetMethod, serviceInstanceFieldBuilder, serviceProviderFieldBuilder, isImplementExplicitly);
        }
    }
}
