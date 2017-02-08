namespace AspectCore.Abstractions.Internal.Test.Fakes
{
    public class ProxyService : TargetService
    {
        public override int Add(int value)
        {
            return base.Add(value);
        }
    }
}
