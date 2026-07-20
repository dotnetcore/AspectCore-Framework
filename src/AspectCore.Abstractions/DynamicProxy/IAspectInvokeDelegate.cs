namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// Represents a statically-generated delegate that invokes the target method implementation
    /// without relying on <see cref="System.Reflection.Emit"/> or expression tree compilation.
    /// Source generators produce implementations of this interface for each intercepted method,
    /// enabling NativeAOT-compatible AOP interception.
    /// </summary>
    [NonAspect]
    public interface IAspectInvokeDelegate
    {
        /// <summary>
        /// Invokes the target method on the given instance with the specified parameters.
        /// </summary>
        /// <param name="instance">The target object instance on which to invoke the method.</param>
        /// <param name="parameters">The method parameters.</param>
        /// <returns>The return value of the invoked method, or <c>null</c> for void methods.</returns>
        object Invoke(object instance, object[] parameters);
    }
}
