using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions.Internal;
using AspectCore.Abstractions.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal class ProxyPropertyGenerator : PropertyGenerator
    {
        protected readonly PropertyInfo _propertyInfo;
        protected readonly Type _serviceType;
        protected readonly Type _parentType;
        protected readonly FieldBuilder _serviceInstanceFieldBuilder;
        protected readonly FieldBuilder _serviceProviderFieldBuilder;
        protected readonly bool _isImplementExplicitly;

        public ProxyPropertyGenerator(TypeBuilder declaringMember, PropertyInfo propertyInfo,
             Type serviceType, Type parentType,
             FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder,
             bool isImplementExplicitly) :
             base(declaringMember)
        {
            _propertyInfo = propertyInfo;
            _serviceType = serviceType;
            _parentType = parentType;
            _serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            _serviceProviderFieldBuilder = serviceProviderFieldBuilder;
            _isImplementExplicitly = isImplementExplicitly;
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
                return _propertyInfo.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _propertyInfo.CanWrite;
            }
        }

        public override MethodInfo GetMethod
        {
            get
            {
                return _propertyInfo.GetGetMethod();
            }
        }

        public override PropertyAttributes PropertyAttributes
        {
            get
            {
                return _propertyInfo.Attributes;
            }
        }

        public override string PropertyName
        {
            get
            {
                return _isImplementExplicitly ? _propertyInfo.GetFullName() : _propertyInfo.Name;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return _propertyInfo.PropertyType;
            }
        }

        public override MethodInfo SetMethod
        {
            get
            {
                return _propertyInfo.GetSetMethod();
            }
        }

        protected override MethodGenerator GetReadMethodGenerator(TypeBuilder declaringType)
        {
            return new ProxyMethodGenerator(declaringType, _serviceType, _parentType, GetMethod, _serviceInstanceFieldBuilder, _serviceProviderFieldBuilder, _isImplementExplicitly);
        }

        protected override MethodGenerator GetWriteMethodGenerator(TypeBuilder declaringType)
        {
            return new ProxyMethodGenerator(declaringType, _serviceType, _parentType, SetMethod, _serviceInstanceFieldBuilder, _serviceProviderFieldBuilder, _isImplementExplicitly);
        }
    }
}
