using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Abstractions;

namespace AspectCore.Core.Generator
{
    [NonAspect]
    public abstract class TypeGenerator : AbstractGenerator<ModuleBuilder, TypeBuilder>
    {
        public abstract string TypeName { get; }

        public abstract TypeAttributes TypeAttributes { get; }

        public abstract Type ParentType { get; }

        public abstract Type[] Interfaces { get; }

        protected TypeGenerator(ModuleBuilder declaringMember) : base(declaringMember)
        {
        }

        public virtual TypeInfo CreateTypeInfo()
        {
            return Build().CreateTypeInfo();
        }

        protected override TypeBuilder ExecuteBuild()
        {
            return DeclaringMember.DefineType(TypeName, TypeAttributes, ParentType, Interfaces);
        }
    }
}
