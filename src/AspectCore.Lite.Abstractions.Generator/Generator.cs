using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class Generator
    {
        private readonly ICollection<Generator> members = new List<Generator>();

        protected virtual ICollection<Generator> Members
        {
            get
            {
                return members;
            }
        }
    }

    public abstract class Generator<TDeclaringMember, TBuilder> : Generator
    {
        public virtual TDeclaringMember DeclaringMember { get; }

        public Generator(TDeclaringMember declaringMember)
        {
            DeclaringMember = declaringMember;
        }

        protected internal abstract TBuilder Accept(GeneratorVisitor visitor);
    }
}
