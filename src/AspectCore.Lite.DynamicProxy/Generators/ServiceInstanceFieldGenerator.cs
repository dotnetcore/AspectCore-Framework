using AspectCore.Lite.Abstractions.Generator;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    internal class ServiceInstanceFieldGenerator : FieldGenerator
    {
        public ServiceInstanceFieldGenerator(TypeBuilder declaringMember, Type serviceType) : base(declaringMember)
        {
            FieldType = serviceType;
        }

        public override FieldAttributes FieldAttributes { get; } = FieldAttributes.Private;

        public override string FieldName { get; } = "proxyField_ServiceInstance";

        public override Type FieldType { get; }
    }
}
