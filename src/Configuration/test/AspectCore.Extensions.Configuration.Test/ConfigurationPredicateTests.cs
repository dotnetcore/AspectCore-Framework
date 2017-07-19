using AspectCore.Abstractions.Extensions;
using Xunit;

namespace AspectCore.Extensions.Configuration.Test
{
    public class ConfigurationPredicateTests
    {
        public string Property_Test { get; set; }

        [Theory]
        [InlineData("AspectCore.Extensions.Configuration.Test.ConfigurationPredicateTests")]
        [InlineData("AspectCore.Extensions.Configuration.Test.*")]
        [InlineData("*.ConfigurationPredicateTests")]
        [InlineData("*")]
        public void ForService_True_Test(string service)
        {
            var predicate = Predicates.ForService(service);
            var method = ReflectionExtensions.GetMethod<ConfigurationPredicateTests>("ForService_True_Test");
            Assert.True(predicate(method));
        }

        [Fact]
        public void ForService_False_Test()
        {
            var predicate = Predicates.ForService("AspectCore.Extensions.Configuration.Test.ConfigurationPredicateTests.x");
            var method = ReflectionExtensions.GetMethod<ConfigurationPredicateTests>("ForService_False_Test");
            Assert.False(predicate(method));
        }

        [Theory]
        [InlineData("ForMethod_Test")]
        [InlineData("ForMethod_*")]
        [InlineData("*_Test")]
        [InlineData("*")]
        public void ForMethod_Test(string methodName)
        {
            var predicate = Predicates.ForMethod(methodName);
            var method = ReflectionExtensions.GetMethod<ConfigurationPredicateTests>("ForMethod_Test");
            Assert.True(predicate(method));
        }

        [Theory]
        [InlineData("AspectCore.Extensions.Configuration.Test.ConfigurationPredicateTests", "ForMethodWithService_Test")]
        [InlineData("AspectCore.Extensions.Configuration.Test.*", "ForMethodWithService_*")]
        [InlineData("*.ConfigurationPredicateTests", "*_Test")]
        [InlineData("*", "*")]
        public void ForMethodWithService_Test(string service, string methodName)
        {
            var predicate = Predicates.ForMethod(service, methodName);
            var method = ReflectionExtensions.GetMethod<ConfigurationPredicateTests>("ForMethodWithService_Test");
            Assert.True(predicate(method));
        }

        [Theory]
        [InlineData("Property_Test")]
        [InlineData("Property_*")]
        [InlineData("*_Test")]
        [InlineData("*")]
        public void ForProperty_Test(string propertyName)
        {
            var predicate = Predicates.ForProperty(propertyName);
            var method = ReflectionExtensions.GetMethod<ConfigurationPredicateTests>("get_Property_Test");
            Assert.True(predicate(method));
        }

        [Theory]
        [InlineData("AspectCore.Extensions.Configuration.Test.ConfigurationPredicateTests", "Property_Test")]
        [InlineData("AspectCore.Extensions.Configuration.Test.*", "Property_*")]
        [InlineData("*.ConfigurationPredicateTests", "*_Test")]
        [InlineData("*", "*")]
        public void ForPropertyWithService_Test(string service, string propertyName)
        {
            var predicate = Predicates.ForProperty(service, propertyName);
            var method = ReflectionExtensions.GetMethod<ConfigurationPredicateTests>("get_Property_Test");
            Assert.True(predicate(method));
        }
    }
}