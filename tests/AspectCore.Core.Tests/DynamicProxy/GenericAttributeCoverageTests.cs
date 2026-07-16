using System;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy;
using Xunit;

#nullable enable

namespace AspectCore.Core.Tests.DynamicProxy
{
    /// <summary>
    /// Tests for the BuildCustomAttribute method in ILEmitVisitor.cs, covering:
    /// - Generic attribute support (C# 11 feature: [SomeAttribute{T}])
    /// - Attributes with constructor arguments but no named arguments
    /// - Attribute filtering (compiler-generated and AspectCore marker attributes
    ///   are NOT copied to the proxy type)
    /// </summary>
    public class GenericAttributeCoverageTests : DynamicProxyTestBase
    {
        // ============================================================
        // 1. Generic attribute support (C# 11 feature)
        //    Exercises the `if (attributeType.IsGenericType)` branch
        //    (lines 638-642) and the fallback at line 657.
        // ============================================================

        [Fact]
        public void InterfaceProxy_WithGenericAttribute_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IGenericAttributeService>();
            Assert.NotNull(proxy);

            // The proxy must be created successfully even with a generic attribute.
            Assert.IsAssignableFrom<IGenericAttributeService>(proxy);
        }

        [Fact]
        public void InterfaceProxy_WithGenericAttribute_MethodAttributeIsReadable()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IGenericAttributeService>();
            Assert.NotNull(proxy);

            // The generic attribute on the method must be forwarded to the proxy
            // and be readable via reflection.
            var method = proxy.GetType().GetMethod(nameof(IGenericAttributeService.Process));
            Assert.NotNull(method);

            var attributes = method!.GetCustomAttributes(true);
            var genericAttr = attributes.FirstOrDefault(a =>
                a.GetType().IsGenericType &&
                a.GetType().GetGenericTypeDefinition() == typeof(GenericTestAttribute<>));

            Assert.NotNull(genericAttr);
        }

        [Fact]
        public void ClassProxy_WithGenericAttribute_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<GenericAttributeService>();
            Assert.NotNull(proxy);

            // The class proxy with a generic attribute must be created successfully.
            var method = proxy.GetType().GetMethod(nameof(GenericAttributeService.Process));
            Assert.NotNull(method);

            var attributes = method!.GetCustomAttributes(true);
            var genericAttr = attributes.FirstOrDefault(a =>
                a.GetType().IsGenericType &&
                a.GetType().GetGenericTypeDefinition() == typeof(GenericTestAttribute<>));

            Assert.NotNull(genericAttr);
        }

        [Fact]
        public void ClassProxy_WithGenericAttribute_TypeAttributeIsReadable()
        {
            var proxy = ProxyGenerator.CreateClassProxy<GenericAttributeService>();
            Assert.NotNull(proxy);

            // The generic attribute on the class must be forwarded to the proxy type.
            var typeAttributes = proxy.GetType().GetCustomAttributes(true);
            var genericAttr = typeAttributes.FirstOrDefault(a =>
                a.GetType().IsGenericType &&
                a.GetType().GetGenericTypeDefinition() == typeof(GenericTestAttribute<>));

            Assert.NotNull(genericAttr);
        }

        // ============================================================
        // 2. Attribute with no named arguments
        //    Exercises line 657 (fallback when NamedArguments is null):
        //    return new CustomAttributeBuilder(constructor,
        //        data.ConstructorArguments.Select(c => c.Value).ToArray());
        // ============================================================

        [Fact]
        public void InterfaceProxy_WithObsoleteAttribute_MethodAttributeIsForwarded()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IObsoleteAttributeService>();
            Assert.NotNull(proxy);

            // The [Obsolete("method reason")] on the method must be forwarded.
            var method = proxy.GetType().GetMethod(nameof(IObsoleteAttributeService.DoWork));
            Assert.NotNull(method);

            var methodAttributes = method!.GetCustomAttributes(true);
            var obsoleteAttr = methodAttributes.OfType<ObsoleteAttribute>().FirstOrDefault();
            Assert.NotNull(obsoleteAttr);
            Assert.Equal("method obsolete", obsoleteAttr!.Message);
        }

        [Fact]
        public void ClassProxy_WithObsoleteAttribute_TypeAttributeIsForwarded()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ObsoleteAttributeService>();
            Assert.NotNull(proxy);

            // The [Obsolete("class reason")] on the class must be forwarded.
            var typeAttributes = proxy.GetType().GetCustomAttributes(true);
            var obsoleteAttr = typeAttributes.OfType<ObsoleteAttribute>().FirstOrDefault();
            Assert.NotNull(obsoleteAttr);
            Assert.Equal("class obsolete", obsoleteAttr!.Message);
        }

        [Fact]
        public void ClassProxy_WithObsoleteAttribute_MethodAttributeIsForwarded()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ObsoleteAttributeService>();
            Assert.NotNull(proxy);

            // The [Obsolete("method obsolete")] on the method must be forwarded.
            var method = proxy.GetType().GetMethod(nameof(ObsoleteAttributeService.DoWork));
            Assert.NotNull(method);

            var methodAttributes = method!.GetCustomAttributes(true);
            var obsoleteAttr = methodAttributes.OfType<ObsoleteAttribute>().FirstOrDefault();
            Assert.NotNull(obsoleteAttr);
            Assert.Equal("method obsolete", obsoleteAttr!.Message);
        }

        // ============================================================
        // 3. Attribute filtering test
        //    Verifies that compiler-generated attributes (NullableContext,
        //    CompilerGenerated) and AspectCore marker attributes (NonAspect)
        //    are NOT duplicated on the proxy type.
        // ============================================================

        [Fact]
        public void ClassProxy_DoesNotCopy_NullableContextAttribute()
        {
            var proxy = ProxyGenerator.CreateClassProxy<NullableContextService>();
            Assert.NotNull(proxy);

            // NullableContextAttribute is a compiler-generated attribute and must
            // NOT be copied onto the proxy type.
            var typeAttributes = proxy.GetType().GetCustomAttributes(true);
            var hasNullableContext = typeAttributes.Any(a =>
                a.GetType().FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            Assert.False(hasNullableContext);
        }

        [Fact]
        public void ClassProxy_DoesNotDuplicate_CompilerGeneratedAttribute()
        {
            var proxy = ProxyGenerator.CreateClassProxy<CompilerGeneratedService>();
            Assert.NotNull(proxy);

            // The proxy generator may add its own [CompilerGenerated] to the
            // proxy type, but the one from the service type must NOT be copied
            // (no duplication).
            var typeAttributes = proxy.GetType().GetCustomAttributes(true);
            var count = typeAttributes.Count(a =>
                a.GetType().FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            // At most one (added by the generator), not two (copied + generated).
            Assert.True(count <= 1,
                $"Expected at most 1 CompilerGeneratedAttribute, found {count}");
        }

        [Fact]
        public void ClassProxy_DoesNotDuplicate_NonAspectAttribute()
        {
            var proxy = ProxyGenerator.CreateClassProxy<NonAspectClassService>();
            Assert.NotNull(proxy);

            // NonAspectAttribute is an AspectCore marker attribute. The proxy
            // generator may add its own [NonAspect] to the proxy type, but the
            // one from the service type must NOT be copied (no duplication).
            var typeAttributes = proxy.GetType().GetCustomAttributes(true);
            var nonAspectCount = typeAttributes.Count(a =>
                a.GetType().FullName == "AspectCore.DynamicProxy.NonAspectAttribute");
            // At most one (added by the generator), not two (copied + generated).
            Assert.True(nonAspectCount <= 1,
                $"Expected at most 1 NonAspectAttribute, found {nonAspectCount}");
        }

        [Fact]
        public void ClassProxy_Preserves_RegularAttributes_WhileFilteringMarkers()
        {
            var proxy = ProxyGenerator.CreateClassProxy<MixedAttributeService>();
            Assert.NotNull(proxy);

            // The Description attribute (a regular attribute) must be forwarded.
            var typeAttributes = proxy.GetType().GetCustomAttributes(true);
            var descriptionAttr = typeAttributes.OfType<System.ComponentModel.DescriptionAttribute>().FirstOrDefault();
            Assert.NotNull(descriptionAttr);
            Assert.Equal("mixed-service", descriptionAttr!.Description);

            // But the NullableContext attribute must NOT be present.
            var hasNullableContext = typeAttributes.Any(a =>
                a.GetType().FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            Assert.False(hasNullableContext);
        }

        // ============================================================
        // Service type definitions
        // ============================================================

        // --- Generic attribute (C# 11 feature) ---

        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Class)]
        public class GenericTestAttribute<T> : Attribute
        {
            public GenericTestAttribute() { }
        }

        [GenericTestAttribute<int>]
        public interface IGenericAttributeService
        {
            [GenericTestAttribute<string>]
            void Process();
        }

        [GenericTestAttribute<int>]
        public class GenericAttributeService : IGenericAttributeService
        {
            [GenericTestAttribute<string>]
            public virtual void Process() { }
        }

        // --- Attribute with no named arguments (constructor args only) ---

        [Obsolete("interface obsolete")]
        public interface IObsoleteAttributeService
        {
            [Obsolete("method obsolete")]
            void DoWork();
        }

        [Obsolete("class obsolete")]
        public class ObsoleteAttributeService
        {
            [Obsolete("method obsolete")]
            public virtual void DoWork() { }
        }

        // --- Attribute filtering: NullableContext ---

        // NullableContextAttribute is emitted by the C# compiler at the module
        // level when nullable reference types are enabled. We use a nullable
        // return type to trigger compiler-emitted nullable attributes, and
        // verify they are filtered from the proxy type.
        public class NullableContextService
        {
            public virtual string? GetName() => null;
        }

        // --- Attribute filtering: CompilerGenerated ---

        [System.Runtime.CompilerServices.CompilerGenerated]
        public class CompilerGeneratedService
        {
            public virtual void DoWork() { }
        }

        // --- Attribute filtering: NonAspect (class-only for type-level check) ---

        [NonAspect]
        public class NonAspectClassService
        {
            public virtual void DoWork() { }
        }

        // --- Mixed attributes: regular + filtered ---

        [System.ComponentModel.Description("mixed-service")]
        public class MixedAttributeService
        {
            public virtual string? GetLabel() => "mixed";
        }
    }
}
