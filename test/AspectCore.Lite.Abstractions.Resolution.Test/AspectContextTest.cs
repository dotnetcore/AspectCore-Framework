using AspectCore.Lite.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Resolution.Test
{
    public class AspectContextTest
    {
        [Fact]
        public void ServiceProvider_Null_Test()
        {
            var type = typeof(object);
            var method = type.GetTypeInfo().GetMethod("Equals", new Type[] { typeof(object) });
            var parameter = method.GetParameters().First();
            var context = new AspectContext(null, new TargetDescriptor(new object(), method, type, method, type), new ProxyDescriptor(new object(), method, type),
                new ParameterCollection(EmptyArray<object>.Value, EmptyArray<ParameterInfo>.Value), new ReturnParameterDescriptor(null, parameter));
            Assert.NotNull(context);
            var exception = Assert.Throws<NotImplementedException>(() => context.ServiceProvider);
            Assert.Equal(exception.Message, "The current context does not support ServiceProvider call.");

        }
    }
}
