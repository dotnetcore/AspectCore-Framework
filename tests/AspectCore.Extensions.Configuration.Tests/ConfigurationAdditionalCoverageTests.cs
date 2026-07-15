using System;
using System.Collections.Generic;
using AspectCore.DependencyInjection;
using AspectCore.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AspectCore.Extensions.Configuration.Tests
{
    public class ConfigurationBindResolveCallbackTests
    {
        private static IServiceResolver BuildResolver(Dictionary<string, string> configValues)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();
            var container = new ServiceContext();
            container.AddInstance<IConfiguration>(configuration);
            return container.Build().Resolve<IServiceResolver>();
        }

        [Fact]
        public void Invoke_NullInstance_ReturnsNull()
        {
            var callback = new ConfigurationBindResolveCallback();
            var resolver = BuildResolver(new Dictionary<string, string> { { "key", "value" } });
            var result = callback.Invoke(resolver, null, null);
            Assert.Null(result);
        }

        [Fact]
        public void Invoke_IConfigurationInstance_ReturnsInstance()
        {
            var callback = new ConfigurationBindResolveCallback();
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "key", "value" } })
                .Build();
            var resolver = BuildResolver(new Dictionary<string, string>());
            var result = callback.Invoke(resolver, configuration, null);
            Assert.Same(configuration, result);
        }

        [Fact]
        public void Invoke_ValueBinding_BindsValue()
        {
            var callback = new ConfigurationBindResolveCallback();
            var resolver = BuildResolver(new Dictionary<string, string>
            {
                { "MyValue", "42" }
            });
            var target = new ValueBindTarget();
            var result = callback.Invoke(resolver, target, null);
            Assert.Equal("42", target.MyValue);
        }

        [Fact]
        public void Invoke_ValueBinding_WithSection_BindsValue()
        {
            var callback = new ConfigurationBindResolveCallback();
            var resolver = BuildResolver(new Dictionary<string, string>
            {
                { "section:MyValue", "hello" }
            });
            var target = new ValueBindTargetWithSection();
            var result = callback.Invoke(resolver, target, null);
            Assert.Equal("hello", target.MyValue);
        }

        [Fact]
        public void Invoke_ClassBinding_BindsClass()
        {
            var callback = new ConfigurationBindResolveCallback();
            var resolver = BuildResolver(new Dictionary<string, string>
            {
                { "MyClass:Name", "test" },
                { "MyClass:Age", "25" }
            });
            var target = new ClassBindTarget();
            var result = callback.Invoke(resolver, target, null);
            Assert.NotNull(target.MyClass);
            Assert.Equal("test", target.MyClass.Name);
            Assert.Equal(25, target.MyClass.Age);
        }

        [Fact]
        public void Invoke_NoMetadataAttributes_ReturnsInstanceUnchanged()
        {
            var callback = new ConfigurationBindResolveCallback();
            var resolver = BuildResolver(new Dictionary<string, string> { { "key", "value" } });
            var target = new NoMetadataTarget { Value = "original" };
            var result = callback.Invoke(resolver, target, null);
            Assert.Equal("original", ((NoMetadataTarget)result).Value);
        }

        [Fact]
        public void Invoke_ValueBinding_IntType_BindsValue()
        {
            var callback = new ConfigurationBindResolveCallback();
            var resolver = BuildResolver(new Dictionary<string, string>
            {
                { "IntValue", "123" }
            });
            var target = new IntBindTarget();
            var result = callback.Invoke(resolver, target, null);
            Assert.Equal(123, target.IntValue);
        }

        public class ValueBindTarget
        {
            [ConfigurationValue("MyValue")]
            public string MyValue;
        }

        public class ValueBindTargetWithSection
        {
            [ConfigurationValue("MyValue", "section")]
            public string MyValue;
        }

        public class ClassBindTarget
        {
            [ConfigurationBinding("MyClass")]
            public MyClassConfig MyClass;
        }

        public class MyClassConfig
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public class NoMetadataTarget
        {
            public string Value;
        }

        public class IntBindTarget
        {
            [ConfigurationValue("IntValue")]
            public int IntValue;
        }
    }

    public class ConfigurationAttributesTests
    {
        [Fact]
        public void ConfigurationBindType_HasExpectedValues()
        {
            Assert.Equal(0, (int)ConfigurationBindType.Value);
            Assert.Equal(1, (int)ConfigurationBindType.Class);
        }

        [Fact]
        public void ConfigurationBindingAttribute_Type_IsClass()
        {
            var attr = new ConfigurationBindingAttribute("section");
            Assert.Equal(ConfigurationBindType.Class, attr.Type);
        }

        [Fact]
        public void ConfigurationBindingAttribute_Key_IsNull()
        {
            var attr = new ConfigurationBindingAttribute("section");
            Assert.Null(attr.Key);
        }

        [Fact]
        public void ConfigurationBindingAttribute_Sections_ReturnsSections()
        {
            var attr = new ConfigurationBindingAttribute("a", "b", "c");
            Assert.Equal(new[] { "a", "b", "c" }, attr.Sections);
        }

        [Fact]
        public void ConfigurationValueAttribute_Type_IsValue()
        {
            var attr = new ConfigurationValueAttribute("key");
            Assert.Equal(ConfigurationBindType.Value, attr.Type);
        }

        [Fact]
        public void ConfigurationValueAttribute_Key_ReturnsKey()
        {
            var attr = new ConfigurationValueAttribute("myKey");
            Assert.Equal("myKey", attr.Key);
        }

        [Fact]
        public void ConfigurationValueAttribute_Sections_ReturnsSections()
        {
            var attr = new ConfigurationValueAttribute("key", "s1", "s2");
            Assert.Equal(new[] { "s1", "s2" }, attr.Sections);
        }

        [Fact]
        public void ConfigurationValueAttribute_NoSections_ReturnsEmpty()
        {
            var attr = new ConfigurationValueAttribute("key");
            Assert.Empty(attr.Sections);
        }
    }

    public class ServiceContainerExtensionsAdditionalTests
    {
        [Fact]
        public void AddConfigurationInject_NullContext_Throws()
        {
            IServiceContext context = null;
            Assert.Throws<ArgumentNullException>(() => context.AddConfigurationInject());
        }

        [Fact]
        public void AddConfigurationInject_ReturnsContext()
        {
            var context = new ServiceContext();
            var result = context.AddConfigurationInject();
            Assert.Same(context, result);
        }

        [Fact]
        public void AddConfigurationInject_RegistersCallback()
        {
            var context = new ServiceContext();
            context.AddConfigurationInject();
            var dict = new Dictionary<string, string>
            {
                { "creator:age", "24" },
                { "creator:name", "lemon" }
            };
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
            context.AddInstance<IConfiguration>(configuration);
            context.AddType<BindConfigService>();
            var service = context.Build().Resolve<BindConfigService>();
            Assert.Equal("lemon-24", service.ToString());
        }
    }
}
