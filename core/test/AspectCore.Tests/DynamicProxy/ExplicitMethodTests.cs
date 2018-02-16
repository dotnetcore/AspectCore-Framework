using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AspectCore.DynamicProxy;
using Xunit;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Tests.DynamicProxy
{
    public class ExplicitMethodTests
    {
        [Fact]
        public void Find_ExplicitMethod()
        {
            var method = typeof(IFakeExplicitMethod<int>).GetMethods().First();
            method = method.MakeGenericMethod(typeof(string));
            var explicitMethod = typeof(FakeExplicitMethod).GetTypeInfo().GetMethodBySignature(method);
            Assert.NotNull(explicitMethod);
        }

        public interface IFakeExplicitMethod<T>
        {
            void Method<K>();
        }

        public class FakeExplicitMethod : IFakeExplicitMethod<int>
        {
            void IFakeExplicitMethod<int>.Method<L>()
            {
                throw new NotImplementedException();
            }
        }
    }
}