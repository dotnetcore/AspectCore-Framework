namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectContextFactory
    {
        AspectContext CreateContext(AspectActivatorContext activatorContext);

        /// <summary>
        /// Creates an <see cref="AspectContext"/> using a source-generated invoke delegate
        /// for NativeAOT-compatible interception. The default implementation falls back to
        /// <see cref="CreateContext(AspectActivatorContext)"/>, preserving backward compatibility
        /// for existing implementations that only override the single-parameter overload.
        /// </summary>
        /// <param name="activatorContext">The activator context containing method and instance metadata.</param>
        /// <param name="invokeDelegate">The source-generated delegate for invoking the target method.</param>
        /// <returns>An <see cref="AspectContext"/> configured for NativeAOT interception.</returns>
        AspectContext CreateContext(AspectActivatorContext activatorContext, IAspectInvokeDelegate invokeDelegate)
            => CreateContext(activatorContext);

        void ReleaseContext(AspectContext aspectContext);
    }
}
