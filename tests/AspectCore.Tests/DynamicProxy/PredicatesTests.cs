using AspectCore.Configuration;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class PredicatesTests
    {
        [Fact]
        public void ImplementBaseClass()
        {
            var predicate = Predicates.Implement(typeof(ServiceBase));
            var method = typeof(Transient).GetMethod(nameof(Transient.Foo));
            Assert.True(predicate(method));
        }
        
        [Fact]
        public void ImplementInterface()
        {
            var predicate = Predicates.Implement(typeof(Tests.IService));
            var method = typeof(Transient).GetMethod("get_Id");
            Assert.True(predicate(method));
        }
    }
}