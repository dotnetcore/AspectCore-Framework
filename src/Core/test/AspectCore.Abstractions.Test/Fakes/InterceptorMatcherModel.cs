namespace AspectCore.Abstractions.Internal.Test.Fakes
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

        public void ConfigureInterceptor()
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
