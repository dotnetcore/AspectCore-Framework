using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.DynamicProxy;
using Xunit;
using System.Reflection;

namespace AspectCore.Tests.DynamicProxy
{
    public class RefParameterTests : DynamicProxyTestBase
    {
        [Fact]
        public void Ref_Test()
        {
            var proxy = ProxyGenerator.CreateClassProxy<FakeRefParameter>();
            string name = "l";
            int age = 0;
            proxy.Ref(ref name, ref age);
            Assert.Equal("lemon", name);
            Assert.Equal(22, age);
        }

        [Fact]
        public void Out_Test()
        {
            var proxy = ProxyGenerator.CreateClassProxy<FakeRefParameter>();
            proxy.Out(out var name);
            Assert.Equal("lemon", name);
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.EnableParameterAspect();
        }
    }

    public class FakeRefParameter
    {
        public virtual void Ref([Ref]ref string name, [Ref]ref int age)
        {
        }

        [OutParameterInterceptor]
        public virtual void Out(out string name)
        {
            name = "l";
        }
    }

    public class OutParameterInterceptor : AbstractInterceptorAttribute
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            await next(context);
            context.Parameters[0] = "lemon";
        }
    }

    public class RefAttribute : ParameterInterceptorAttribute
    {
        public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
        {
            var parameter = context.Parameter;

            if (parameter.ParameterInfo.ParameterType == typeof(string).MakeByRefType())
            {
                parameter.Value = "lemon";
            }
            if (parameter.ParameterInfo.ParameterType == typeof(int).MakeByRefType())
            {
                parameter.Value = 22;
            }

            return next(context);
        }
    }
}