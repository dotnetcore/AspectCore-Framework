using AspectCore.Lite.Abstractions.Generator;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    internal class ServiceProviderFieldGenerator : FieldGenerator
    {
        public ServiceProviderFieldGenerator(TypeBuilder declaringMember) : base(declaringMember)
        {
        }

        public override FieldAttributes FieldAttributes { get; } = FieldAttributes.Private;

        public override string FieldName { get; } = "proxyField_ServiceProvider";

        public override Type FieldType { get; } = typeof(IServiceProvider);
    }
}
