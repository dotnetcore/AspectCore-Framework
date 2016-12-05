using AspectCore.Lite.Abstractions.Generator;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AspectCore.Lite.DynamicProxy.Generators
{
    public class ProxyTypeGenerator : TypeGenerator
    {
        private readonly Type serviceType;

        public ProxyTypeGenerator(ModuleBuilder declaringMember, Type serviceType, Type parentType) : base(declaringMember)
        {

            this.serviceType = serviceType;
            this.ParentType = parentType;
        }

        public override Type[] Interfaces { get; } = System.Type.EmptyTypes;

        public override Type ParentType { get; }

        public override TypeBuilder Type
        {
            get
            {
                throw new NotImplementedException();
            }

            protected set
            {
                throw new NotImplementedException();
            }
        }

        public override TypeAttributes TypeAttributes { get; } = TypeAttributes.Class | TypeAttributes.Public;

        public override string TypeName
        {
            get
            {
                return $"{serviceType.Namespace}.Proxy_{serviceType.Name}";
            }
        }

        protected override TypeBuilder InitializeTypeBuilder()
        {
            return base.InitializeTypeBuilder();
        }
    }
}
