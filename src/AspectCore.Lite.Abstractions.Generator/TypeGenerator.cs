using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class TypeGenerator : MetaDataGenerator<ModuleBuilder, TypeBuilder>
    {
        public abstract TypeBuilder Type { get; protected set; }

        public abstract string TypeName { get; }

        public abstract TypeAttributes TypeAttributes { get; }

        public abstract Type ParentType { get; }

        public abstract Type[] Interfaces { get; }

        public TypeGenerator(ModuleBuilder declaringMember) : base(declaringMember)
        {
        }

        protected virtual TypeBuilder InitializeTypeBuilder()
        {
            return DeclaringMember.DefineType(TypeName, TypeAttributes, ParentType, Interfaces);
        }

        public virtual void AddMember(MetaDataGenerator generator)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }
            Members.Add(generator);
        }

        public virtual TypeInfo CreateTypeInfo()
        {
            return Type.CreateTypeInfo();
        }

        protected override TypeBuilder Accept(GeneratorVisitor visitor)
        {
            foreach(var member in Members)
            {
                visitor.VisitGenerator(member);
            }
            return Type;
        }
    }
}
