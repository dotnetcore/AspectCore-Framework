using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Core.Generator;

namespace AspectCore.Core.Internal.Generator
{
    internal class ProxyMethodGenerator : GenericMethodGenerator
    {
        const MethodAttributes ExplicitMethodAttributes = MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        const MethodAttributes OverrideMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;

        protected readonly Type _serviceType;
        protected readonly Type _parentType;
        protected readonly MethodInfo _serviceMethod;
        protected readonly FieldBuilder _serviceInstanceFieldBuilder;
        protected readonly FieldBuilder _serviceProviderFieldBuilder;
        protected readonly bool _isImplementExplicitly;

        public ProxyMethodGenerator(TypeBuilder declaringMember, Type serviceType, Type parentType, MethodInfo serviceMethod,
            FieldBuilder serviceInstanceFieldBuilder, FieldBuilder serviceProviderFieldBuilder, bool isImplementExplicitly) : base(declaringMember)
        {
            _serviceType = serviceType;
            _parentType = parentType;
            _serviceMethod = serviceMethod;
            _serviceInstanceFieldBuilder = serviceInstanceFieldBuilder;
            _serviceProviderFieldBuilder = serviceProviderFieldBuilder;
            _isImplementExplicitly = isImplementExplicitly;
        }

        public override CallingConventions CallingConventions
        {
            get
            {
                return _serviceMethod.CallingConvention;
            }
        }

        public override MethodAttributes MethodAttributes
        {
            get
            {
                if (_serviceType.GetTypeInfo().IsInterface)
                {
                    return ExplicitMethodAttributes;
                }

                var attributes = OverrideMethodAttributes;

                if (_serviceMethod.Attributes.HasFlag(MethodAttributes.Public))
                {
                    attributes = attributes | MethodAttributes.Public;
                }

                if (_serviceMethod.Attributes.HasFlag(MethodAttributes.Family))
                {
                    attributes = attributes | MethodAttributes.Family;
                }

                if (_serviceMethod.Attributes.HasFlag(MethodAttributes.FamORAssem))
                {
                    attributes = attributes | MethodAttributes.FamORAssem;
                }

                return attributes;
            }
        }

        public override string MethodName
        {
            get
            {
                return _isImplementExplicitly ? _serviceMethod.GetFullName() : _serviceMethod.Name;
            }
        }

        public override Type[] ParameterTypes
        {
            get
            {
                return _serviceMethod.GetParameterTypes();
            }
        }

        public override Type ReturnType
        {
            get
            {
                return _serviceMethod.ReturnType;
            }
        }

        public override bool IsGenericMethod
        {
            get
            {
                return _serviceMethod.IsGenericMethod;
            }
        }

        protected override MethodBuilder ExecuteBuild()
        {
            var builder = base.ExecuteBuild();

            if (_serviceType.GetTypeInfo().IsInterface)
            {
                DeclaringMember.DefineMethodOverride(builder, _serviceMethod);
            }

            return builder;
        }

        protected override MethodBodyGenerator GetMethodBodyGenerator(MethodBuilder declaringMethod)
        {
            var parentMethod = _parentType.GetTypeInfo().GetMethodBySign(_serviceMethod);
            return new ProxyMethodBodyGenerator(declaringMethod,
                DeclaringMember,
                _serviceType,
                _serviceMethod,
                parentMethod ?? _serviceMethod,
                _serviceInstanceFieldBuilder,
                _serviceProviderFieldBuilder);
        }

        protected internal override void GeneratingGenericParameter(MethodBuilder declaringMethod)
        {
            var genericArguments = _serviceMethod.GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
            var genericArgumentsBuilders = declaringMethod.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (var index = 0; index < genericArguments.Length; index++)
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(genericArguments[index].GenericParameterAttributes);
                foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                }
            }
        }
    }
}
