using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;

namespace AspectCore.Extensions.Windsor
{
    public class CompatibleCollectionResolver : CollectionResolver
    {
        public CompatibleCollectionResolver(IKernel kernel)
            : base(kernel, allowEmptyCollections: true)
        {
        }

        public override bool CanResolve(CreationContext context, ISubDependencyResolver contextHandlerResolver, ComponentModel model,
            DependencyModel dependency)
        {
            if (kernel.HasComponent(dependency.TargetItemType))
            {
                return false;
            }

            return base.CanResolve(context, contextHandlerResolver, model, dependency);
        }
    }
}
