using AspectCore.Abstractions.Internal;
using Xunit;

namespace AspectCore.Abstractions.Test
{
    public class MatchTest
    {
        [Theory]
        [InlineData("*")]
        [InlineData("AspectCore.Abstractions.*")]
        [InlineData("*.Abstractions.*")]
        [InlineData("*.Abstractions.Test")]
        [InlineData("AspectCore.Abstractions.Test")]
        public void Match_True_Test(string vaule)
        {
            const string fake = "AspectCore.Abstractions.Test";
            Assert.True(fake.Matches(vaule));
        }

        [Theory]
        [InlineData("AspectCore.Abstractions.Test1")]
        [InlineData("AspectCore.Abstractions.")]
        [InlineData("AspectCore.Lite.*.Abstractions.Test")]
        [InlineData("*.AspectCore.Abstractions.Test")]
        public void Match_False_Test(string vaule)
        {
            const string fake = "AspectCore.Abstractions.Test";
            Assert.False(fake.Matches(vaule));
        }
    }
}
