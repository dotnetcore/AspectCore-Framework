using AspectCore.Lite.Abstractions.Extensions;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test
{
    public class MatchTest
    {
        [Theory]
        [InlineData("*")]
        [InlineData("AspectCore.Lite.Abstractions.*")]
        [InlineData("*.Abstractions.*")]
        [InlineData("*.Abstractions.Test")]
        [InlineData("AspectCore.Lite.Abstractions.Test")]
        public void Match_True_Test(string vaule)
        {
            const string fake = "AspectCore.Lite.Abstractions.Test";
            Assert.True(fake.Matches(vaule));
        }

        [Theory]
        [InlineData("AspectCore.Lite.Abstractions.Test1")]
        [InlineData("AspectCore.Lite.Abstractions.")]
        [InlineData("AspectCore.Lite.*.Abstractions.Test")]
        [InlineData("*.AspectCore.Lite.Abstractions.Test")]
        public void Match_False_Test(string vaule)
        {
            const string fake = "AspectCore.Lite.Abstractions.Test";
            Assert.False(fake.Matches(vaule));
        }
    }
}
