using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.Windsor
{
    [NonAspect]
    internal class WindsorAspectBuilderFactory : IAspectBuilderFactory
    {
        private readonly IAspectBuilderFactory _aspectBuilderFactory;
        private readonly AspectDelegate _complete;

        public WindsorAspectBuilderFactory(IAspectBuilderFactory aspectBuilderFactory, AspectDelegate complete)
        {
            _aspectBuilderFactory = aspectBuilderFactory;
            _complete = complete;
        }

        public IAspectBuilder Create(AspectContext context)
        {
            var builder = _aspectBuilderFactory.Create(context);
            return new AspectBuilder(_complete, builder.Delegates.ToList());
        }

        public IAspectBuilder GetBuilder(MethodInfo serviceMethod, MethodInfo implementationMethod)
        {
            var builder = _aspectBuilderFactory.GetBuilder(serviceMethod, implementationMethod);
            return new AspectBuilder(_complete, builder.Delegates.ToList());
        }

        public IAspectBuilder GetBuilder(MethodInfo serviceMethod, MethodInfo implementationMethod, MethodInfo predicateMethod)
        {
            var builder = _aspectBuilderFactory.GetBuilder(serviceMethod, implementationMethod, predicateMethod);
            return new AspectBuilder(_complete, builder.Delegates.ToList());
        }
    }
}