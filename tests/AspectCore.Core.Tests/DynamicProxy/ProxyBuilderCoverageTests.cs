using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.ProxyBuilder.Builders;
using AspectCore.DynamicProxy.ProxyBuilder.Nodes;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyBuilderCoverageTests
    {
        private static IAspectValidator CreateValidator()
        {
            var builder = new AspectValidatorBuilder(new AspectConfiguration());
            return builder.Build();
        }

        #region ClassProxyBuilder - No suitable constructor

        [Fact]
        public void ClassProxyBuilder_NoSuitableConstructor_ThrowsInvalidOperationException()
        {
            // Tests lines 91-93: type with no public/protected constructors
            var validator = CreateValidator();
            var builder = new ClassProxyBuilder(
                "TestProxy",
                typeof(NoConstructorClass),
                typeof(NoConstructorClass),
                Type.EmptyTypes,
                validator);
            Assert.Throws<InvalidOperationException>(() => builder.Build());
        }

        #endregion

        #region ClassProxyBuilder - Covariant return property getter paths

        [Fact]
        public void ClassProxyBuilder_WithCovariantReturnProperty_BuildsSuccessfully()
        {
            // Tests FindCovariantReturnPropertyGetter paths
            // Line 255: property.CanWrite → return (null, false) - property has setter
            // Line 283: getter.DeclaringType != getter.ReflectedType → skip
            // Line 286: default return (null, false)
            var validator = CreateValidator();
            var builder = new ClassProxyBuilder(
                "CovariantPropertyProxy",
                typeof(CovariantPropertyBase),
                typeof(CovariantPropertyDerived),
                Type.EmptyTypes,
                validator);
            var nodes = builder.Build();
            Assert.NotNull(nodes);
            Assert.Single(nodes);
        }

        [Fact]
        public void ClassProxyBuilder_WithReadOnlyCovariantProperty_BuildsSuccessfully()
        {
            // Tests the read-only covariant property path
            var validator = CreateValidator();
            var builder = new ClassProxyBuilder(
                "ReadOnlyCovariantProxy",
                typeof(ReadOnlyCovariantBase),
                typeof(ReadOnlyCovariantDerived),
                Type.EmptyTypes,
                validator);
            var nodes = builder.Build();
            Assert.NotNull(nodes);
            Assert.Single(nodes);
        }

        #endregion

        #region ClassProxyBuilder - Additional interface members

        [Fact]
        public void ClassProxyBuilder_WithAdditionalInterface_BuildsSuccessfully()
        {
            // Tests BuildAdditionalInterfaceMembers (lines 325-332, 356)
            // Line 356: FindCovariantReturnGetter with CanRead=false or CanWrite=true → return null
            var validator = CreateValidator();
            var builder = new ClassProxyBuilder(
                "WithInterfaceProxy",
                typeof(ClassWithInterface),
                typeof(ClassWithInterface),
                new[] { typeof(IAdditional) },
                validator);
            var nodes = builder.Build();
            Assert.NotNull(nodes);
            Assert.Single(nodes);
        }

        [Fact]
        public void ClassProxyBuilder_WithAdditionalInterfaceHavingProperties_BuildsSuccessfully()
        {
            // Tests property building in additional interfaces
            // The class must implement the interface for ResolveImplementationMethod to work
            var validator = CreateValidator();
            var builder = new ClassProxyBuilder(
                "WithPropsInterfaceProxy",
                typeof(ClassWithInterfaceAndProps),
                typeof(ClassWithInterfaceAndProps),
                new[] { typeof(IAdditionalWithProperties) },
                validator);
            var nodes = builder.Build();
            Assert.NotNull(nodes);
            Assert.Single(nodes);
        }

        #endregion

        #region ClassProxyBuilder - Constructors with various access levels

        [Fact]
        public void ClassProxyBuilder_WithProtectedConstructor_BuildsSuccessfully()
        {
            // Tests line 137: constructor with protected access
            var validator = CreateValidator();
            var builder = new ClassProxyBuilder(
                "ProtectedCtorProxy",
                typeof(ProtectedConstructorClass),
                typeof(ProtectedConstructorClass),
                Type.EmptyTypes,
                validator);
            var nodes = builder.Build();
            Assert.NotNull(nodes);
            Assert.Single(nodes);
        }

        #endregion

        #region InterfaceImplBuilder - Build paths

        [Fact]
        public void InterfaceImplBuilder_Build_ReturnsStubAndProxyNodes()
        {
            // Tests lines 36-41: Build() with Distinct/ToArray and BuildStubType/BuildProxyType
            var validator = CreateValidator();
            var builder = new InterfaceImplBuilder(
                "TestImpl",
                "TestProxy",
                typeof(ISimple),
                Type.EmptyTypes,
                validator);
            var nodes = builder.Build();
            Assert.NotNull(nodes);
            Assert.Equal(2, nodes.Length); // stub + proxy
        }

        [Fact]
        public void InterfaceImplBuilder_BuildStubOnly_ReturnsSingleNode()
        {
            // Tests BuildStubType path (lines 174-176 indirectly)
            var validator = CreateValidator();
            var builder = new InterfaceImplBuilder(
                "StubOnlyImpl",
                "StubOnlyProxy",
                typeof(ISimple),
                Type.EmptyTypes,
                validator);
            var stubNode = builder.BuildStubOnly();
            Assert.NotNull(stubNode);
        }

        [Fact]
        public void InterfaceImplBuilder_BuildProxyOnly_ReturnsSingleNode()
        {
            // Tests BuildProxyTypeWithStub path
            // Use a type that implements the interface as the stub impl type
            var validator = CreateValidator();
            var builder = new InterfaceImplBuilder(
                "ProxyOnlyImpl",
                "ProxyOnlyProxy",
                typeof(ISimple),
                Type.EmptyTypes,
                validator);
            var proxyNode = builder.BuildProxyOnly("ProxyName", typeof(SimpleImpl));
            Assert.NotNull(proxyNode);
        }

        [Fact]
        public void InterfaceImplBuilder_WithAdditionalInterfaces_Deduplicates()
        {
            // Tests Distinct() on additional interfaces (line 37)
            // The primary interface must implement the additional interface methods
            // or the additional interface must be empty
            var validator = CreateValidator();
            var builder = new InterfaceImplBuilder(
                "DedupImpl",
                "DedupProxy",
                typeof(ISimple),
                new[] { typeof(IEmptyAdditional), typeof(IEmptyAdditional) }, // duplicate
                validator);
            var nodes = builder.Build();
            Assert.NotNull(nodes);
            Assert.Equal(2, nodes.Length);
        }

        #endregion

        #region InterfaceImplBuilder - ResolveImplementationMethod paths

        [Fact]
        public void ResolveImplementationMethod_SameDeclaringType_ReturnsMethod()
        {
            // Tests line 405: method.DeclaringType == implType → return method
            var method = typeof(ISimple).GetMethod("DoSomething");
            var result = InterfaceImplBuilder.ResolveImplementationMethod(method, typeof(ISimple));
            Assert.NotNull(result);
            Assert.Equal(method, result);
        }

        [Fact]
        public void ResolveImplementationMethod_GenericTypeDefinitionMatch_ReturnsMethod()
        {
            // Tests lines 414-415: generic type definition match
            var method = typeof(IGenericSvc<>).GetMethod("GetValue");
            var result = InterfaceImplBuilder.ResolveImplementationMethod(method, typeof(IGenericSvc<>));
            Assert.NotNull(result);
        }

        [Fact]
        public void ResolveImplementationMethod_FoundViaGetMethodBySignature_ReturnsMethod()
        {
            // Tests line 418-419: GetMethodBySignature finds the method
            var method = typeof(ISimple).GetMethod("DoSomething");
            var result = InterfaceImplBuilder.ResolveImplementationMethod(method, typeof(SimpleImpl));
            Assert.NotNull(result);
        }

        [Fact]
        public void ResolveImplementationMethod_FoundViaInterfaceSearch_ReturnsMethod()
        {
            // Tests line 433: found in interface search
            var method = typeof(IAdditional).GetMethod("AdditionalMethod");
            var result = InterfaceImplBuilder.ResolveImplementationMethod(method, typeof(ImplWithMultipleInterfaces));
            Assert.NotNull(result);
        }

        [Fact]
        public void ResolveImplementationMethod_NotFoundNoInterfaces_ThrowsMissingMethodException()
        {
            // Tests line 424: no interfaces → throw MissingMethodException
            // Use a method that doesn't exist on NoInterfaceClass
            var method = typeof(ISimple).GetMethod("DoSomething");
            // NoInterfaceClass has DoSomething(), so use a different method that won't be found
            var differentMethod = typeof(IAdditional).GetMethod("AdditionalMethod");
            Assert.Throws<MissingMethodException>(() =>
                InterfaceImplBuilder.ResolveImplementationMethod(differentMethod, typeof(NoInterfaceClass)));
        }

        [Fact]
        public void ResolveImplementationMethod_NotFoundInAnyInterface_ThrowsMissingMethodException()
        {
            // Tests line 435: not found in any interface → throw MissingMethodException
            var method = typeof(ISimple).GetMethod("DoSomething");
            Assert.Throws<MissingMethodException>(() =>
                InterfaceImplBuilder.ResolveImplementationMethod(method, typeof(ImplWithUnrelatedInterface)));
        }

        [Fact]
        public void ResolveImplementationMethod_FoundViaAbstractInterceptorInterface_ReturnsMethod()
        {
            // Tests the abstract interfaces search path (line 426-427)
            var method = typeof(IAbstractIntercepted).GetMethod("AbstractMethod");
            var result = InterfaceImplBuilder.ResolveImplementationMethod(method, typeof(AbstractInterceptedImpl));
            Assert.NotNull(result);
        }

        #endregion

        #region Test Types

        // Class with no public/protected constructors
        public class NoConstructorClass
        {
            private NoConstructorClass() { }
        }

        // Class with protected constructor
        public class ProtectedConstructorClass
        {
            protected ProtectedConstructorClass() { }
            public virtual void Method() { }
        }

        // Covariant return property types
        public class BaseResult { }
        public class DerivedResult : BaseResult { }

        public class CovariantPropertyBase
        {
            public virtual BaseResult Result { get; set; } = new BaseResult();
            public virtual BaseResult ReadOnly { get; } = new BaseResult();
        }

        public class CovariantPropertyDerived : CovariantPropertyBase
        {
            // Covariant return - overrides with more derived type
            public override DerivedResult ReadOnly => new DerivedResult();
        }

        public class ReadOnlyCovariantBase
        {
            public virtual BaseResult Value { get; } = new BaseResult();
        }

        public class ReadOnlyCovariantDerived : ReadOnlyCovariantBase
        {
            public override DerivedResult Value => new DerivedResult();
        }

        // Class implementing additional interface
        public class ClassWithInterface : IAdditional
        {
            public virtual void DoSomething() { }
            public void AdditionalMethod() { }
        }

        // Class implementing additional interface with properties
        public class ClassWithInterfaceAndProps : IAdditionalWithProperties
        {
            public virtual void DoSomething() { }
            public int Count { get; set; }
            public string Name => "test";
        }

        public interface IAdditional
        {
            void AdditionalMethod();
        }

        public interface IEmptyAdditional { }

        public interface IAdditionalWithProperties
        {
            int Count { get; set; }
            string Name { get; }
        }

        // Interface types
        public interface ISimple
        {
            void DoSomething();
        }

        public class SimpleImpl : ISimple
        {
            public void DoSomething() { }
        }

        public interface IGenericSvc<T>
        {
            T GetValue();
        }

        public class ImplWithMultipleInterfaces : ISimple, IAdditional
        {
            public void DoSomething() { }
            public void AdditionalMethod() { }
        }

        public class NoInterfaceClass
        {
            public void DoSomething() { }
        }

        public interface IUnrelated { void Unrelated(); }
        public class ImplWithUnrelatedInterface : IUnrelated
        {
            public void Unrelated() { }
        }

        // Abstract interceptor attribute test - use concrete subclass since AbstractInterceptorAttribute is abstract
        [ConcreteInterceptor]
        public interface IAbstractIntercepted
        {
            void AbstractMethod();
        }

        public class AbstractInterceptedImpl : IAbstractIntercepted
        {
            public void AbstractMethod() { }
        }

        // Concrete interceptor attribute for testing
        public class ConcreteInterceptorAttribute : AbstractInterceptorAttribute
        {
            public override System.Threading.Tasks.Task Invoke(AspectContext context, AspectDelegate next)
                => next(context);
        }

        #endregion
    }
}
