namespace AspectCore.Lite.Abstractions
{
    /// <summary>
    /// ImplementType of IInjectable is searched method named FromService , injection type you need in the parameters of the method.
    /// 
    /// for example :
    /// class InjectionInterceptorAttribute : InterceptorAttribute
    /// {   
    ///     private IService1 service1;
    ///     private IService2 service2;
    ///     public void FromService(IService1 service1, IService2 service2, ...)
    ///     {
    ///         this.service1 = service1;
    ///         this.service2 = service2;
    ///         ...
    ///     }
    ///     
    ///     public override void Execute(AspectContext aspectContext, InterceptorDelegate next)
    ///     {
    ///     }
    /// }
    /// 
    /// In this case, the service1 and service2 will be automatically injected.
    /// </summary>
    public interface IFromServiceable
    {
    }
}
