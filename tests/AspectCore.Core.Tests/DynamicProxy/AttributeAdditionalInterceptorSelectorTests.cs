using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AttributeAdditionalInterceptorSelectorExtendedTests
    {
        [Fact]
        public void Select_WithSameServiceAndImplementationMethod_ReturnsEmpty()
        {
            var selector = new AttributeAdditionalInterceptorSelector();
            var method = typeof(TestServiceImpl).GetMethod(nameof(TestServiceImpl.DoSomething));
            var result = selector.Select(method, method);
            Assert.Empty(result);
        }

        [Fact]
        public void Select_WithInterfaceMethodAndImplementation_ReturnsEmptyWhenNoAttributes()
        {
            var selector = new AttributeAdditionalInterceptorSelector();
            var interfaceMethod = typeof(ITestService).GetMethod(nameof(ITestService.DoSomething));
            var implMethod = typeof(TestServiceImpl).GetMethod(nameof(TestServiceImpl.DoSomething));
            var result = selector.Select(interfaceMethod, implMethod);
            Assert.Empty(result);
        }

        [Fact]
        public void Select_WithClassLevelInterceptorAttribute_ReturnsInterceptor()
        {
            var selector = new AttributeAdditionalInterceptorSelector();
            var interfaceMethod = typeof(ITestService).GetMethod(nameof(ITestService.DoSomething));
            var implMethod = typeof(InterceptedServiceImpl).GetMethod(nameof(InterceptedServiceImpl.DoSomething));
            var result = selector.Select(interfaceMethod, implMethod).ToList();
            Assert.Single(result);
            Assert.IsType<TestInterceptorAttribute>(result[0]);
        }

        [Fact]
        public void Select_WithMethodLevelInterceptorAttribute_ReturnsInterceptor()
        {
            var selector = new AttributeAdditionalInterceptorSelector();
            var interfaceMethod = typeof(ITestService).GetMethod(nameof(ITestService.DoSomething));
            var implMethod = typeof(MethodInterceptedServiceImpl).GetMethod(nameof(MethodInterceptedServiceImpl.DoSomething));
            var result = selector.Select(interfaceMethod, implMethod).ToList();
            Assert.Single(result);
            Assert.IsType<TestInterceptorAttribute>(result[0]);
        }

        public interface ITestService
        {
            void DoSomething();
        }

        public class TestServiceImpl : ITestService
        {
            public void DoSomething() { }
        }

        [TestInterceptor]
        public class InterceptedServiceImpl : ITestService
        {
            public void DoSomething() { }
        }

        public class MethodInterceptedServiceImpl : ITestService
        {
            [TestInterceptor]
            public void DoSomething() { }
        }

        public class TestInterceptorAttribute : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next) => next(context);
        }
    }
}
