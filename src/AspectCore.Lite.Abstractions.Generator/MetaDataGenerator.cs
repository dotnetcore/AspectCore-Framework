using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class MetaDataGenerator
    {
        private readonly ICollection<MetaDataGenerator> members = new List<MetaDataGenerator>();

        protected virtual ICollection<MetaDataGenerator> Members
        {
            get
            {
                return members;
            }
        }
    }

    public abstract class MetaDataGenerator<TDeclaringMember, TBuilder> : MetaDataGenerator
    {
        public virtual TDeclaringMember DeclaringMember { get; }

        public MetaDataGenerator(TDeclaringMember declaringMember)
        {
            DeclaringMember = declaringMember;
        }

        protected abstract TBuilder Accept(GeneratorVisitor visitor);
    }
}
