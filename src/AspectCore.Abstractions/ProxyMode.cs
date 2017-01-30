namespace AspectCore.Abstractions
{
    public enum ProxyMode
    {
        /// <summary>
        /// Inject service instances as proxy targets
        /// </summary>
        ServiceInstance,

        /// <summary>
        /// Inherit the implementation class as proxy target
        /// </summary>
        Inheritance
    }
}
