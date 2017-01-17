namespace AspectCore.Lite.Abstractions.Resolution.Test.Fakes
{
    public class InstanceSupportOriginalService: InstanceServiceProvider, IOriginalServiceProvider
    {
        public InstanceSupportOriginalService(object instance) : base(instance)
        {
        }

        public void Dispose()
        {
        }
    }
}
