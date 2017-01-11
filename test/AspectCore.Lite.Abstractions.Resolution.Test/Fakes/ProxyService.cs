namespace AspectCore.Lite.Abstractions.Resolution.Test.Fakes
{
    public class ProxyService : TargetService
    {
        public override int Add(int value)
        {
            return base.Add(value);
        }
    }
}
