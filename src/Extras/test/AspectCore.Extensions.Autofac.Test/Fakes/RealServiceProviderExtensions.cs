using AspectCore.Abstractions;

namespace AspectCore.Extensions.Autofac.Test.Fakes
{
    public static class RealServiceProviderExtensions
    {
        public static T GetService<T>(this IRealServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }
    }
}
