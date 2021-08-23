using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;
using AspectCore.DynamicProxy;

namespace AspectCore.Tests.DynamicProxy
{
    public class DefineAttributesTests : DynamicProxyTestBase
    {
        [Description(nameof(Service)), DefaultValue(nameof(Service))]
        public class Service
        {
            [Description(nameof(OnMethod)), DefaultValue(nameof(OnMethod))]
            public void OnMethod() { }

            [Description(nameof(OnProperty)), DefaultValue(nameof(OnProperty))]
            public virtual string OnProperty { get; set; }

            [return: Description(nameof(OnReturn)), DefaultValue(nameof(OnReturn))]
            public int OnReturn() => 1;

            public void OnParameter([Description(nameof(OnParameter)), DefaultValue(nameof(OnParameter))] int arg) { }

            public void OnGenericArgument<[Description(nameof(OnGenericArgument)), DefaultValue(nameof(OnGenericArgument))] T>() { }
        }

        private static void CheckAttribute<T>(IEnumerable<object> attributes, Action<T> check) where T : Attribute
        {
            var attribute = (T)attributes.FirstOrDefault(m => m.GetType() == typeof(T));
            Assert.NotNull(attribute);
            check(attribute);
        }

        private static void CheckAttributes(IEnumerable<object> attributes, string value)
        {
            CheckAttribute<DescriptionAttribute>(attributes, m => Assert.Equal(value, m.Description));
            CheckAttribute<DefaultValueAttribute>(attributes, m => Assert.Equal(value, m.Value));
        }

        [Fact]
        public void Attributes_OnClass_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            var attributes = service.GetType().GetCustomAttributes(true);
            CheckAttributes(attributes, nameof(Service));
        }

        [Fact]
        public void Attributes_OnMethod_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            var attributes = service.GetType().GetMethod(nameof(Service.OnMethod)).GetCustomAttributes(true);
            CheckAttributes(attributes, nameof(Service.OnMethod));
        }

        [Fact]
        public void Attributes_OnProperty_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            var attributes = service.GetType().GetProperty(nameof(Service.OnProperty)).GetCustomAttributes(true);
            CheckAttributes(attributes, nameof(Service.OnProperty));
        }

        [Fact]
        public void Attributes_OnReturn_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            var attributes = service.GetType().GetMethod(nameof(Service.OnReturn)).ReturnParameter.GetCustomAttributes(true);
            CheckAttributes(attributes, nameof(Service.OnReturn));
        }

        [Fact]
        public void Attributes_OnParameter_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            var attributes = service.GetType().GetMethod(nameof(Service.OnParameter)).GetParameters().Single().GetCustomAttributes(true);
            CheckAttributes(attributes, nameof(Service.OnParameter));
        }

        [Fact]
        public void Attributes_OnGenericArgument_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            var attributes = service.GetType().GetMethod(nameof(Service.OnGenericArgument)).GetGenericArguments().Single().GetCustomAttributes(true);
            CheckAttributes(attributes, nameof(Service.OnGenericArgument));
        }
    }
}
