using AspectCore.DependencyInjection;

namespace AspectCore.Configuration
{
    public sealed class AspectConfiguration : IAspectConfiguration
    {
        public AspectValidationHandlerCollection ValidationHandlers { get; }

        public InterceptorCollection Interceptors { get; }

        public NonAspectPredicateCollection NonAspectPredicates { get; }

        public bool ThrowAspectException { get; set; }

        public AspectConfiguration()
        {
            ThrowAspectException = true;
            ValidationHandlers = new AspectValidationHandlerCollection().AddDefault(this);
            Interceptors = new InterceptorCollection();
            NonAspectPredicates = new NonAspectPredicateCollection().AddDefault();
        }
    }
}