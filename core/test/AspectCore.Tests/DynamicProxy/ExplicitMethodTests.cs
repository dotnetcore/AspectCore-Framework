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
            var method = typeof(IFakeExplicitMethod<int>).GetMethod("Method");
            var m = method.MakeGenericMethod(typeof(string));
            var t = m.DeclaringType;
            var explicitMethod = typeof(FakeExplicitMethod).GetTypeInfo().GetMethodBySignature(method);
            var d = explicitMethod.GetReflector().DisplayName;
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