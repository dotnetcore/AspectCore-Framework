using System;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyGeneratorExtensionsTests : DynamicProxyTestBase
    {
        #region CreateClassProxy(Type, Type)

        [Fact]
        public void CreateClassProxy_TypeType_ReturnsProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy(serviceType: typeof(ExtTestClass), implementationType: typeof(ExtTestClass));
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<ExtTestClass>(proxy);
            Assert.IsNotType<ExtTestClass>(proxy);
            Assert.True(((ExtTestClass)proxy).IsProxy());
        }

        [Fact]
        public void CreateClassProxy_TypeType_InterceptsMethod()
        {
            var proxy = (ExtTestClass)ProxyGenerator.CreateClassProxy(serviceType: typeof(ExtTestClass), implementationType: typeof(ExtTestClass));
            Assert.Equal("intercepted", proxy.GetName());
        }

        [Fact]
        public void CreateClassProxy_TypeType_PassesThroughNonInterceptedMethod()
        {
            var proxy = (ExtTestClass)ProxyGenerator.CreateClassProxy(serviceType: typeof(ExtTestClass), implementationType: typeof(ExtTestClass));
            Assert.Equal(7, proxy.Add(3, 4));
        }

        [Fact]
        public void CreateClassProxy_TypeType_NullProxyGenerator_ThrowsArgumentNullException()
        {
            IProxyGenerator generator = null;
            Assert.Throws<ArgumentNullException>(() =>
                generator.CreateClassProxy(serviceType: typeof(ExtTestClass), implementationType: typeof(ExtTestClass)));
        }

        [Fact]
        public void CreateClassProxy_TypeType_NullServiceType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ProxyGenerator.CreateClassProxy(serviceType: null, implementationType: typeof(ExtTestClass)));
        }

        [Fact]
        public void CreateClassProxy_TypeType_NullImplementationType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ProxyGenerator.CreateClassProxy(serviceType: typeof(ExtTestClass), implementationType: null));
        }

        [Fact]
        public void CreateClassProxy_TypeType_InterfaceServiceType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                ProxyGenerator.CreateClassProxy(serviceType: typeof(IExtTestService), implementationType: typeof(ExtTestService)));
        }

        #endregion

        #region CreateClassProxy(Type, object[])

        [Fact]
        public void CreateClassProxy_TypeArgs_ReturnsProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy(typeof(ExtTestClassWithCtor), new object[] { "hello" });
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<ExtTestClassWithCtor>(proxy);
            Assert.IsNotType<ExtTestClassWithCtor>(proxy);
        }

        [Fact]
        public void CreateClassProxy_TypeArgs_PassesConstructorArguments()
        {
            var proxy = (ExtTestClassWithCtor)ProxyGenerator.CreateClassProxy(
                typeof(ExtTestClassWithCtor), new object[] { "hello" });
            Assert.Equal("hello", proxy.ConstructorArg);
        }

        [Fact]
        public void CreateClassProxy_TypeArgs_InterceptsMethod()
        {
            var proxy = (ExtTestClassWithCtor)ProxyGenerator.CreateClassProxy(
                typeof(ExtTestClassWithCtor), new object[] { "hello" });
            Assert.Equal("intercepted", proxy.GetName());
        }

        [Fact]
        public void CreateClassProxy_TypeArgs_NullArgs_TreatedAsEmpty()
        {
            var proxy = ProxyGenerator.CreateClassProxy(typeof(ExtTestClass), (object[])null);
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<ExtTestClass>(proxy);
        }

        [Fact]
        public void CreateClassProxy_TypeArgs_NoArgs_UsesParameterlessConstructor()
        {
            var proxy = ProxyGenerator.CreateClassProxy(typeof(ExtTestClass));
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<ExtTestClass>(proxy);
        }

        [Fact]
        public void CreateClassProxy_TypeArgs_NullProxyGenerator_ThrowsArgumentNullException()
        {
            IProxyGenerator generator = null;
            Assert.Throws<ArgumentNullException>(() =>
                generator.CreateClassProxy(typeof(ExtTestClass), new object[] { "hello" }));
        }

        #endregion

        #region CreateInterfaceProxy(Type)

        [Fact]
        public void CreateInterfaceProxy_Type_ReturnsProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy(typeof(IExtTestService));
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<IExtTestService>(proxy);
        }

        [Fact]
        public void CreateInterfaceProxy_Type_InterceptsMethod()
        {
            var proxy = (IExtTestService)ProxyGenerator.CreateInterfaceProxy(typeof(IExtTestService));
            Assert.Equal("intercepted", proxy.GetName());
        }

        [Fact]
        public void CreateInterfaceProxy_Type_ReturnsDefault_WithoutImplementation()
        {
            // Interface proxy without an implementation instance returns default values
            // for non-intercepted methods (no underlying implementation to delegate to).
            var proxy = (IExtTestService)ProxyGenerator.CreateInterfaceProxy(typeof(IExtTestService));
            Assert.Equal(0, proxy.Add(3, 4));
        }

        [Fact]
        public void CreateInterfaceProxy_Type_NullServiceType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ProxyGenerator.CreateInterfaceProxy(null));
        }

        [Fact]
        public void CreateInterfaceProxy_Type_ClassServiceType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                ProxyGenerator.CreateInterfaceProxy(typeof(ExtTestClass)));
        }

        #endregion

        #region CreateInterfaceProxy(Type, Type)

        [Fact]
        public void CreateInterfaceProxy_TypeType_ReturnsProxy()
        {
            var proxy = ProxyGeneratorExtensions.CreateInterfaceProxy(
                ProxyGenerator, typeof(IExtTestService), typeof(ExtTestService));
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<IExtTestService>(proxy);
        }

        [Fact]
        public void CreateInterfaceProxy_TypeType_UsesImplementationInstance()
        {
            var proxy = (IExtTestService)ProxyGeneratorExtensions.CreateInterfaceProxy(
                ProxyGenerator, typeof(IExtTestService), typeof(ExtTestService));
            Assert.Equal("intercepted", proxy.GetName());
        }

        [Fact]
        public void CreateInterfaceProxy_TypeType_PassesThroughNonInterceptedMethod()
        {
            var proxy = (IExtTestService)ProxyGeneratorExtensions.CreateInterfaceProxy(
                ProxyGenerator, typeof(IExtTestService), typeof(ExtTestService));
            Assert.Equal(7, proxy.Add(3, 4));
        }

        [Fact]
        public void CreateInterfaceProxy_TypeType_WithConstructorArgs()
        {
            var proxy = (IExtTestService)ProxyGeneratorExtensions.CreateInterfaceProxy(
                ProxyGenerator, typeof(IExtTestService), typeof(ExtTestServiceWithCtor), new object[] { "ctor-value" });
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<IExtTestService>(proxy);
        }

        [Fact]
        public void CreateInterfaceProxy_TypeType_NullProxyGenerator_ThrowsArgumentNullException()
        {
            IProxyGenerator generator = null;
            Assert.Throws<ArgumentNullException>(() =>
                ProxyGeneratorExtensions.CreateInterfaceProxy(generator, typeof(IExtTestService), typeof(ExtTestService)));
        }

        [Fact]
        public void CreateInterfaceProxy_TypeType_NullServiceType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ProxyGeneratorExtensions.CreateInterfaceProxy(ProxyGenerator, null, typeof(ExtTestService)));
        }

        [Fact]
        public void CreateInterfaceProxy_TypeType_NullImplementationType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ProxyGeneratorExtensions.CreateInterfaceProxy(ProxyGenerator, typeof(IExtTestService), null));
        }

        [Fact]
        public void CreateInterfaceProxy_TypeType_ClassServiceType_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                ProxyGeneratorExtensions.CreateInterfaceProxy(ProxyGenerator, typeof(ExtTestClass), typeof(ExtTestClass)));
        }

        #endregion

        #region Generic overloads (delegating to Type-based overloads)

        [Fact]
        public void CreateClassProxy_Generic_ServiceImplementation_ReturnsProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ExtTestClass, ExtTestClass>();
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<ExtTestClass>(proxy);
            Assert.IsNotType<ExtTestClass>(proxy);
        }

        [Fact]
        public void CreateClassProxy_Generic_Implementation_ReturnsProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ExtTestClass>();
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<ExtTestClass>(proxy);
            Assert.Equal("intercepted", proxy.GetName());
        }

        [Fact]
        public void CreateInterfaceProxy_Generic_Service_ReturnsProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IExtTestService>();
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<IExtTestService>(proxy);
            Assert.Equal("intercepted", proxy.GetName());
        }

        [Fact]
        public void CreateInterfaceProxy_Generic_ServiceImplementation_ReturnsProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IExtTestService, ExtTestService>();
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<IExtTestService>(proxy);
            Assert.Equal("intercepted", proxy.GetName());
        }

        [Fact]
        public void CreateInterfaceProxy_Generic_WithInstance_ReturnsProxy()
        {
            var instance = new ExtTestService();
            var proxy = ProxyGenerator.CreateInterfaceProxy<IExtTestService>(instance);
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<IExtTestService>(proxy);
            Assert.Equal("intercepted", proxy.GetName());
        }

        [Fact]
        public void CreateClassProxy_Generic_NullProxyGenerator_ThrowsArgumentNullException()
        {
            IProxyGenerator generator = null;
            Assert.Throws<ArgumentNullException>(() => generator.CreateClassProxy<ExtTestClass>());
        }

        [Fact]
        public void CreateClassProxy_GenericServiceImpl_NullProxyGenerator_ThrowsArgumentNullException()
        {
            IProxyGenerator generator = null;
            Assert.Throws<ArgumentNullException>(() => generator.CreateClassProxy<IExtTestService, ExtTestService>());
        }

        [Fact]
        public void CreateInterfaceProxy_Generic_NullProxyGenerator_ThrowsArgumentNullException()
        {
            IProxyGenerator generator = null;
            Assert.Throws<ArgumentNullException>(() => generator.CreateInterfaceProxy<IExtTestService>());
        }

        [Fact]
        public void CreateInterfaceProxy_GenericWithInstance_NullProxyGenerator_ThrowsArgumentNullException()
        {
            IProxyGenerator generator = null;
            Assert.Throws<ArgumentNullException>(() => generator.CreateInterfaceProxy<IExtTestService>(new ExtTestService()));
        }

        #endregion

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await next(ctx);
                if (ctx.ServiceMethod.Name == "GetName")
                {
                    ctx.ReturnValue = "intercepted";
                }
            }, Predicates.ForService("*ExtTest*"));
        }

        #region Test Types

        public interface IExtTestService
        {
            string GetName();
            int Add(int a, int b);
        }

        public class ExtTestService : IExtTestService
        {
            public string GetName() => "original";
            public int Add(int a, int b) => a + b;
        }

        public class ExtTestServiceWithCtor : IExtTestService
        {
            public string ConstructorArg { get; }

            public ExtTestServiceWithCtor(string arg)
            {
                ConstructorArg = arg;
            }

            public string GetName() => "original";
            public int Add(int a, int b) => a + b;
        }

        public class ExtTestClass
        {
            public virtual string GetName() => "original";
            public virtual int Add(int a, int b) => a + b;
        }

        public class ExtTestClassWithCtor : ExtTestClass
        {
            public string ConstructorArg { get; }

            public ExtTestClassWithCtor(string arg)
            {
                ConstructorArg = arg;
            }
        }

        #endregion
    }
}
