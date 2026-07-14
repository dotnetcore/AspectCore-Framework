using System;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AdditionalCoreCoverageTests : DynamicProxyTestBase
    {
        private readonly ProxyTypeGenerator _proxyTypeGenerator;

        public AdditionalCoreCoverageTests()
        {
            var configuration = new AspectCore.Configuration.AspectConfiguration();
            var validatorBuilder = new AspectValidatorBuilder(configuration);
            _proxyTypeGenerator = new ProxyTypeGenerator(validatorBuilder);
        }

        // ============================================================
        // InterfaceImplAstBuilder.Build() method (lines 36-41)
        // Triggered when creating interface proxy WITH implementation type
        // ============================================================

        [Fact]
        public void InterfaceProxyType_WithImplementationType_CallsBuild()
        {
            var proxyType = _proxyTypeGenerator.CreateInterfaceProxyType(
                typeof(ISimpleService), typeof(SimpleServiceImpl));
            Assert.NotNull(proxyType);
            Assert.True(typeof(ISimpleService).IsAssignableFrom(proxyType));
        }

        [Fact]
        public void InterfaceProxyType_WithImplementationType_AndAdditionalInterfaces_CallsBuild()
        {
            var proxyType = _proxyTypeGenerator.CreateInterfaceProxyType(
                typeof(ICombinedService), typeof(CombinedServiceImpl));
            Assert.NotNull(proxyType);
        }

        // ============================================================
        // ResolveImplementationMethod with generic types (lines 414-415)
        // Triggered when method.DeclaringType is generic and implType is generic type definition
        // ============================================================

        [Fact]
        public void InterfaceProxyType_OpenGenericWithImplementation_TriggersGenericResolve()
        {
            var proxyType = _proxyTypeGenerator.CreateInterfaceProxyType(
                typeof(IGenericService<>), typeof(GenericServiceImpl<>));
            Assert.NotNull(proxyType);
            Assert.True(proxyType.IsGenericTypeDefinition);

            // Create a closed generic instance
            var closedType = proxyType.MakeGenericType(typeof(string));
            var instance = Activator.CreateInstance(closedType, new object[] { null, new GenericServiceImpl<string>() });
            Assert.NotNull(instance);
            Assert.IsAssignableFrom<IGenericService<string>>(instance);
        }

        // ============================================================
        // ResolveImplementationMethod - MissingMethodException (line 424)
        // Triggered when implementation type has no interfaces
        // ============================================================

        [Fact]
        public void InterfaceProxyType_ImplementationWithoutInterface_ThrowsMissingMethod()
        {
            Assert.Throws<MissingMethodException>(() =>
                _proxyTypeGenerator.CreateInterfaceProxyType(
                    typeof(ISimpleService), typeof(CompletelyUnrelated)));
        }

        // ============================================================
        // ClassProxyAstBuilder - no suitable constructor (lines 91-93)
        // Triggered when class has no public/protected constructors
        // ============================================================

        [Fact]
        public void ClassProxyType_NoSuitableConstructor_ThrowsInvalidOperation()
        {
            Assert.Throws<InvalidOperationException>(() =>
                _proxyTypeGenerator.CreateClassProxyType(
                    typeof(NoPublicConstructor), typeof(NoPublicConstructor)));
        }

        // ============================================================
        // ClassProxyAstBuilder - FamORAssem method attribute (line 137)
        // Triggered when a method has FamORAssem (internal protected) access
        // ============================================================

        [Fact]
        public void ClassProxyType_WithInternalProtectedMethod_GeneratesProxy()
        {
            var proxyType = _proxyTypeGenerator.CreateClassProxyType(
                typeof(ProtectedInternalMethod), typeof(ProtectedInternalMethod));
            Assert.NotNull(proxyType);
        }

        // ============================================================
        // ProxyTypeGenerator.GetInterfaces with generic parameters (lines 79-82)
        // Triggered when getting interfaces for a generic type with generic interfaces
        // ============================================================

        [Fact]
        public void ClassProxyType_GenericTypeWithGenericInterface_TriggersGenericInterfaceCheck()
        {
            var proxyType = _proxyTypeGenerator.CreateClassProxyType(
                typeof(GenericWithInterfaceImpl<>), typeof(GenericWithInterfaceImpl<>));
            Assert.NotNull(proxyType);
        }

        // ============================================================
        // TypeExtensions - Covariant return type edge cases
        // ============================================================

        [Fact]
        public void ClassProxy_CovariantReturnWithGenericParameters_GeneratesProxy()
        {
            var service = ProxyGenerator.CreateClassProxy<GenericCovariantService>();
            Assert.NotNull(service);
            var result = service.GetItem();
            Assert.NotNull(result);
        }

        [Fact]
        public void ClassProxy_CovariantReturnFromBaseClass_GeneratesProxy()
        {
            var service = ProxyGenerator.CreateClassProxy<DerivedCovariantService>();
            Assert.NotNull(service);
            var result = service.GetItem();
            Assert.NotNull(result);
        }

        // ============================================================
        // ClassProxyAstBuilder - covariant return getter from base class
        // (lines 283, 286)
        // ============================================================

        [Fact]
        public void ClassProxy_CovariantReturnPropertyFromBase_GeneratesProxy()
        {
            var service = ProxyGenerator.CreateClassProxy<DerivedCovariantWithProperty>();
            Assert.NotNull(service);
            var result = service.Item;
            Assert.NotNull(result);
        }

        // ============================================================
        // InterfaceImplAstBuilder - BuildProxyType (lines 174-176)
        // ============================================================

        [Fact]
        public void InterfaceProxyType_WithoutImplementation_CreatesStubAndProxy()
        {
            var proxyType = _proxyTypeGenerator.CreateInterfaceProxyType(typeof(ISimpleService));
            Assert.NotNull(proxyType);
            Assert.True(typeof(ISimpleService).IsAssignableFrom(proxyType));
        }

        // ============================================================
        // Service type definitions
        // ============================================================

        public interface ISimpleService
        {
            void DoSomething();
            int GetValue();
        }

        public class SimpleServiceImpl : ISimpleService
        {
            public void DoSomething() { }
            public int GetValue() => 42;
        }

        public interface ICombinedService : ISimpleService
        {
            string GetName();
        }

        public class CombinedServiceImpl : ICombinedService
        {
            public void DoSomething() { }
            public int GetValue() => 42;
            public string GetName() => "test";
        }

        // Class with no interfaces (for MissingMethodException test)
        public class NoInterfaceImpl
        {
            public void DoSomething() { }
            public int GetValue() => 42;
        }

        // Class with completely unrelated methods (for MissingMethodException test)
        public class CompletelyUnrelated
        {
            public string UnrelatedMethod() => "unrelated";
        }

        // Class with no public/protected constructors
        public class NoPublicConstructor
        {
            private NoPublicConstructor() { }
            public virtual int GetValue() => 0;
        }

        // Class with internal protected method
        public class ProtectedInternalMethod
        {
            protected internal virtual int GetValue() => 42;
            public virtual int GetPublicValue() => 100;
        }

        // Generic interface
        public interface IGenericService<T>
        {
            T GetItem();
            void SetItem(T item);
        }

        public class GenericServiceImpl<T> : IGenericService<T>
        {
            public T GetItem() => default;
            public void SetItem(T item) { }
        }

        // Generic type with generic interface
        public interface IGenericInterface<T> { }

        public class GenericWithInterfaceImpl<T> : IGenericInterface<T>
        {
            public virtual T GetItem() => default;
        }

        // Covariant return types
        public class BaseItem { public string Name { get; set; } = "base"; }
        public class DerivedItem : BaseItem { public DerivedItem() { Name = "derived"; } }

        public class BaseCovariantService
        {
            public virtual BaseItem GetItem() => new BaseItem();
        }

        public class GenericCovariantService : BaseCovariantService
        {
            // Override with covariant return type
            public override DerivedItem GetItem() => new DerivedItem();
        }

        public class DerivedCovariantService : GenericCovariantService
        {
            // Further override with covariant return type
            public override DerivedItem GetItem() => new DerivedItem { Name = "leaf" };
        }

        // Covariant return with property from base class
        public class BaseWithProperty
        {
            public virtual BaseItem Item => new BaseItem();
        }

        public class DerivedCovariantWithProperty : BaseWithProperty
        {
            public override DerivedItem Item => new DerivedItem();
        }
    }
}
