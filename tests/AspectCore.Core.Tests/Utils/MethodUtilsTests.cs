using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Utils;
using Xunit;

namespace AspectCore.Core.Tests.Utils
{
    public class MethodUtilsTests
    {
        [Fact]
        public void CreateAspectActivator_IsCorrectMethod()
        {
            Assert.NotNull(MethodUtils.CreateAspectActivator);
            Assert.Equal(nameof(IAspectActivatorFactory.Create), MethodUtils.CreateAspectActivator.Name);
            Assert.Equal(typeof(IAspectActivatorFactory), MethodUtils.CreateAspectActivator.DeclaringType);
        }

        [Fact]
        public void AspectActivatorInvoke_IsCorrectMethod()
        {
            Assert.NotNull(MethodUtils.AspectActivatorInvoke);
            Assert.Equal(nameof(IAspectActivator.Invoke), MethodUtils.AspectActivatorInvoke.Name);
        }

        [Fact]
        public void AspectActivatorInvokeTask_IsCorrectMethod()
        {
            Assert.NotNull(MethodUtils.AspectActivatorInvokeTask);
            Assert.Equal(nameof(IAspectActivator.InvokeTask), MethodUtils.AspectActivatorInvokeTask.Name);
        }

        [Fact]
        public void AspectActivatorInvokeValueTask_IsCorrectMethod()
        {
            Assert.NotNull(MethodUtils.AspectActivatorInvokeValueTask);
            Assert.Equal(nameof(IAspectActivator.InvokeValueTask), MethodUtils.AspectActivatorInvokeValueTask.Name);
        }

        [Fact]
        public void AspectActivatorInvokeAsyncEnumerable_IsCorrectMethod()
        {
            Assert.NotNull(MethodUtils.AspectActivatorInvokeAsyncEnumerable);
            Assert.Equal(nameof(IAspectActivator.InvokeAsyncEnumerable), MethodUtils.AspectActivatorInvokeAsyncEnumerable.Name);
        }

        [Fact]
        public void AspectActivatorContextCtor_IsCorrectConstructor()
        {
            Assert.NotNull(MethodUtils.AspectActivatorContextCtor);
            Assert.Equal(typeof(AspectActivatorContext), MethodUtils.AspectActivatorContextCtor.DeclaringType);
        }

        [Fact]
        public void ObjectCtor_IsCorrectConstructor()
        {
            Assert.NotNull(MethodUtils.ObjectCtor);
            Assert.Equal(typeof(object), MethodUtils.ObjectCtor.DeclaringType);
        }

        [Fact]
        public void GetParameters_IsCorrectMethod()
        {
            Assert.NotNull(MethodUtils.GetParameters);
            Assert.Equal("get_Parameters", MethodUtils.GetParameters.Name);
        }

        [Fact]
        public void GetMethodReflector_IsCorrectMethod()
        {
            Assert.NotNull(MethodUtils.GetMethodReflector);
        }

        [Fact]
        public void ReflectorInvoke_IsCorrectMethod()
        {
            Assert.NotNull(MethodUtils.ReflectorInvoke);
        }
    }
}
