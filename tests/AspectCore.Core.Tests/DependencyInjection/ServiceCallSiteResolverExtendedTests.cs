using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceCallSiteResolverExtendedTests
    {
        [Fact]
        public void Resolve_WithTypeServiceThatCannotBeConstructed_ThrowsInvalidOperationException()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var callSiteResolver = new ServiceCallSiteResolver(table);

            // Type with no public constructor that can be resolved
            var def = new TypeServiceDefinition(typeof(INoConstructor), typeof(NoConstructorImpl), Lifetime.Transient);
            var resolver = new ServiceResolver(context);
            Assert.Throws<InvalidOperationException>(() => callSiteResolver.Resolve(def)(resolver));
        }

        [Fact]
        public void Resolve_WithEnumerableServiceWithMultipleElements_ReturnsAllElements()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var callSiteResolver = new ServiceCallSiteResolver(table);

            var elementDefs = new ServiceDefinition[]
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient),
                new InstanceServiceDefinition(typeof(IDisposable), new DisposableImpl()),
            };
            var def = new EnumerableServiceDefinition(
                typeof(IEnumerable<IDisposable>), typeof(IDisposable), elementDefs);
            var resolver = new ServiceResolver(context);
            var result = callSiteResolver.Resolve(def)(resolver);
            Assert.NotNull(result);
            var array = Assert.IsType<IDisposable[]>(result);
            Assert.Equal(2, array.Length);
        }

        [Fact]
        public void Resolve_WithManyEnumerableService_ReturnsManyEnumerable()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var callSiteResolver = new ServiceCallSiteResolver(table);

            var elementDefs = new ServiceDefinition[]
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient),
            };
            var def = new ManyEnumerableServiceDefinition(
                typeof(IManyEnumerable<IDisposable>), typeof(IDisposable), elementDefs);
            var resolver = new ServiceResolver(context);
            var result = callSiteResolver.Resolve(def)(resolver);
            Assert.NotNull(result);
            Assert.IsAssignableFrom<IManyEnumerable<IDisposable>>(result);
        }

        [Fact]
        public void Resolve_WithEmptyEnumerableService_ReturnsEmptyArray()
        {
            var context = new ServiceContext();
            var table = new ServiceTable(context);
            var callSiteResolver = new ServiceCallSiteResolver(table);

            var def = new EnumerableServiceDefinition(
                typeof(IEnumerable<IDisposable>), typeof(IDisposable), new ServiceDefinition[0]);
            var resolver = new ServiceResolver(context);
            var result = callSiteResolver.Resolve(def)(resolver);
            Assert.NotNull(result);
            var array = Assert.IsType<IDisposable[]>(result);
            Assert.Empty(array);
        }

        public interface INoConstructor { }

        public class NoConstructorImpl : INoConstructor
        {
            private NoConstructorImpl() { }
        }

        public class DisposableImpl : IDisposable
        {
            public void Dispose() { }
        }
    }
}
