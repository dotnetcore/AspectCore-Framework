using System;
using System.Reflection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class OpenGenericMethodTests : DynamicProxyTestBase
    {
        [Fact]
        public void OpenGenericInterfaceProxyType_Without_ImplType_Supports_Generic_Method_Invocation()
        {
            var proxyType = ProxyGenerator.TypeGenerator.CreateInterfaceProxyType(typeof(IGenericMethodService<>));
            var closedProxyType = proxyType.MakeGenericType(typeof(int));
            var proxy = (IGenericMethodService<int>)Activator.CreateInstance(closedProxyType, GetAspectActivatorFactory());

            proxy.State = 7;

            Assert.Equal(7, proxy.State);
            Assert.Null(proxy.Transform<string>(7, "fallback"));

            var transformMethod = closedProxyType.GetMethod(nameof(IGenericMethodService<int>.Transform));
            Assert.NotNull(transformMethod);
            Assert.True(transformMethod.IsGenericMethodDefinition);
            Assert.Single(transformMethod.GetGenericArguments());
        }

        [Fact]
        public void ClosedClassProxy_Supports_Generic_Method_Invocation()
        {
            var proxy = ProxyGenerator.CreateClassProxy<GenericMethodService<int>>();

            proxy.State = 11;

            Assert.Equal(11, proxy.State);
            Assert.Equal("11:fallback", proxy.Transform<string>(11, "fallback"));

            var transformMethod = proxy.GetType().GetMethod(nameof(GenericMethodService<int>.Transform));
            Assert.NotNull(transformMethod);
            Assert.True(transformMethod.IsGenericMethodDefinition);
            Assert.Single(transformMethod.GetGenericArguments());
        }

        private IAspectActivatorFactory GetAspectActivatorFactory()
        {
            var proxyGenerator = ProxyGenerator;
            var innerGeneratorField = proxyGenerator.GetType().GetField("_proxyGenerator", BindingFlags.Instance | BindingFlags.NonPublic);
            if (innerGeneratorField?.GetValue(proxyGenerator) is IProxyGenerator innerGenerator)
            {
                proxyGenerator = innerGenerator;
            }

            var aspectActivatorFactoryField = proxyGenerator.GetType().GetField("_aspectActivatorFactory", BindingFlags.Instance | BindingFlags.NonPublic);
            return (IAspectActivatorFactory)aspectActivatorFactoryField.GetValue(proxyGenerator);
        }

        public interface IGenericMethodService<TState>
        {
            TReturn Transform<TReturn>(TState state, TReturn fallback)
                where TReturn : class;

            TState State { get; set; }
        }

        public class GenericMethodService<TState>
        {
            public virtual TReturn Transform<TReturn>(TState state, TReturn fallback)
                where TReturn : class
            {
                return $"{state}:{fallback}" as TReturn;
            }

            public virtual TState State { get; set; }
        }
    }
}
