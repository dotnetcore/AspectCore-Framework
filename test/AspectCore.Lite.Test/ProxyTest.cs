using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Test
{
    public class ProxyTest
    {
        [Fact]
        public void Proxy_Test()
        {
            var proxyService = typeof(ProxyService);
            var method = MethodHelper.GetMethodInfo<Action<ProxyService>>(x => x.Func());
            var isntance = new ProxyService();
            var activator = Expression.Lambda<Func<Proxy>>(Expression.New(
                 typeof(Proxy).GetTypeInfo().DeclaredConstructors.FirstOrDefault(),
                 new Expression[] { Expression.Constant(isntance), Expression.Constant(method), Expression.Constant(proxyService) })).Compile();
            var proxy = activator();
            Assert.Equal(proxy.Instance, isntance);
            Assert.Equal(proxy.Method, method);
            Assert.Equal(proxy.ProxyType, proxyService);
        }

        public class ProxyService
        {
            public void Func()
            {
            }
        }
    }
}
