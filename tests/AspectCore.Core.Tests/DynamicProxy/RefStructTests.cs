using System;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class RefStructTests : DynamicProxyTestBase
    {
        [Fact]
        public void RefStruct_As_ImplementationType_Should_Throw()
        {
            Assert.Throws<NotSupportedException>(() =>
                ProxyGenerator.TypeGenerator.CreateInterfaceProxyType(typeof(ISpanInterface), typeof(SpanImpl)));
        }

        [Fact]
        public void RefStruct_As_ClassProxy_ServiceType_Should_Throw()
        {
            Assert.Throws<NotSupportedException>(() =>
                ProxyGenerator.TypeGenerator.CreateClassProxyType(typeof(SpanImpl), typeof(SpanImpl)));
        }

        [Fact]
        public void RefStruct_Rejection_Message_Should_Be_Clear()
        {
            var ex = Assert.Throws<NotSupportedException>(() =>
                ProxyGenerator.TypeGenerator.CreateClassProxyType(typeof(SpanImpl), typeof(SpanImpl)));
            Assert.Contains("ref struct", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Span", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Regular_Struct_Should_Not_Be_Rejected_As_RefStruct()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                ProxyGenerator.TypeGenerator.CreateClassProxyType(typeof(RegularStruct), typeof(RegularStruct)));
            Assert.DoesNotContain("ref struct", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void Interface_Not_A_RefStruct_Should_Not_Throw()
        {
            var ex = Record.Exception(() =>
                ProxyGenerator.TypeGenerator.CreateInterfaceProxyType(typeof(ISpanInterface)));
            Assert.False(ex is NotSupportedException, "Interface should not be rejected as ref struct");
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) => next(ctx));
        }
    }

    public interface ISpanInterface
    {
        int GetLength();
    }

    public ref struct SpanImpl : ISpanInterface
    {
        private readonly Span<byte> _data;

        public SpanImpl(Span<byte> data)
        {
            _data = data;
        }

        public int GetLength() => _data.Length;
    }

    public struct RegularStruct
    {
        public int Value { get; set; }
    }
}
