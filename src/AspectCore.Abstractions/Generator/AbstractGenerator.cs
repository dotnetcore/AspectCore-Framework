using System;

namespace AspectCore.Abstractions.Generator
{
    public abstract class AbstractGenerator<TDeclaringMember, TBuilder>
    {
        private object _buildLock = new object();
        private TBuilder _builder;

        public virtual TDeclaringMember DeclaringMember { get; }

        public AbstractGenerator(TDeclaringMember declaringMember)
        {
            if (declaringMember == null)
            {
                throw new ArgumentNullException(nameof(declaringMember));
            }

            DeclaringMember = declaringMember;
        }

        public TBuilder Build()
        {
            if (_builder == null)
            {
                lock (_buildLock)
                {
                    if (_builder == null)
                    {
                        _builder = ExecuteBuild();
                    }
                }
            }
            return _builder;
        }

        protected abstract TBuilder ExecuteBuild();
    }
}
