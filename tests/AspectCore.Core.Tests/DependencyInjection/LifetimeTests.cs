using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class LifetimeTests
    {
        [Fact]
        public void Singleton_HasValueZero()
        {
            Assert.Equal(0, (int)Lifetime.Singleton);
        }

        [Fact]
        public void Scoped_HasValueOne()
        {
            Assert.Equal(1, (int)Lifetime.Scoped);
        }

        [Fact]
        public void Transient_HasValueTwo()
        {
            Assert.Equal(2, (int)Lifetime.Transient);
        }

        [Fact]
        public void Singleton_IsFirstValue()
        {
            Assert.Equal(Lifetime.Singleton, (Lifetime)0);
        }

        [Fact]
        public void AllValues_AreDefined()
        {
            Assert.Equal(3, System.Enum.GetValues(typeof(Lifetime)).Length);
        }
    }
}
