using System;
using AspectCore.Abstractions;

namespace AspectCore.Core.Generator
{
    [NonAspect]
    public abstract class AbstractGenerator<TDeclaringMember, TBuilder> where TDeclaringMember : class where TBuilder : class
    {
        private readonly object _buildLock = new object();
        private TBuilder _builder;

        public virtual TDeclaringMember DeclaringMember { get; }

        protected AbstractGenerator(TDeclaringMember declaringMember)
        {
            if (declaringMember == null)
            {
                throw new ArgumentNullException(nameof(declaringMember));
            }
            DeclaringMember = declaringMember;
        }

        public TBuilder Build()
        {
            if (_builder != null) return _builder;
            lock (_buildLock)
            {
                if (_builder == null)
                {
                    _builder = ExecuteBuild();
                }
            }
            return _builder;
        }

        protected abstract TBuilder ExecuteBuild();
    }
}