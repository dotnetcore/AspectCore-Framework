using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test.Issues
{
    // https://github.com/dotnetcore/AspectCore-Framework/issues/287
    public class ParamWithInTests
    {
        public interface IParamWithIn
        {
            object TestFunc(string val, in ReadOnlyMemory<byte> bytes);
        }

        public class ParamWithIn : IParamWithIn
        {
            public object TestFunc(string val, in ReadOnlyMemory<byte> bytes)
            {
                return bytes;
            }
        }

        [Fact]
        public void ParamWithIn_Test()
        {
            var sp = new ServiceCollection()
                .AddScoped<IParamWithIn, ParamWithIn>()
                .BuildDynamicProxyProvider();

            var service = sp.GetRequiredService<IParamWithIn>();
            ReadOnlyMemory<byte> bytes = new ReadOnlyMemory<byte>(new byte[] {0, 1, 2});
            var obj = service.TestFunc("val1", bytes);
            Assert.NotNull(obj);
            Assert.Equal(bytes, obj);
        }
    }
}