namespace AspectCore.Lite.Abstractions.Resolution.Test.Fakes
{
    public class InterceptorMatcherModel
    {
        [MatcherTest]
        public void WithInterceptor()
        {
        }

        public void WithOutInterceptor()
        {
        }
    }

    [MatcherTest]
    public class WithInterceptorMatcherModel
    {
        public void WithOutInterceptor()
        {
        }
    }

    [MatcherTest, MultipMatcherTest]
    public class MultipWithInterceptorMatcherModel
    {
        [MultipMatcherTest]
        public void MultipWithInterceptor()
        {
        }

        [MatcherTest]
        public void WithInterceptor()
        {
        }
    }
}
