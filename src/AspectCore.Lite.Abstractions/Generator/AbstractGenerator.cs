namespace AspectCore.Lite.Abstractions.Generator
{
    public abstract class AbstractGenerator<TDeclaringMember, TBuilder>
    {
        private object buildLock = new object();
        private TBuilder builder;

        public virtual TDeclaringMember DeclaringMember { get; }

        public AbstractGenerator(TDeclaringMember declaringMember)
        {
            DeclaringMember = declaringMember;
        }

        public TBuilder Build()
        {
            if (builder == null)
            {
                lock (buildLock)
                {
                    if (builder == null)
                    {
                        builder = ExecuteBuild();
                    }
                }
            }
            return builder;
        }

        protected abstract TBuilder ExecuteBuild();
    }
}
