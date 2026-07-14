using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyMetadataParityTests : DynamicProxyTestBase
    {
        [Fact]
        public void InterfaceProxy_Preserves_Behavior_And_Metadata()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IParityService, ParityInterfaceService>();

            proxy.Name = "interface";

            Assert.Equal("interface", proxy.Name);
            Assert.Equal("1:payload", proxy.Combine<string>(1, "payload"));

            AssertProxyMetadata(
                typeof(IParityService).GetProperty(nameof(IParityService.Name)),
                proxy.GetType().GetProperty(nameof(IParityService.Name)));
            AssertProxyMetadata(
                typeof(IParityService).GetMethod(nameof(IParityService.Combine)),
                proxy.GetType().GetMethod(nameof(IParityService.Combine)));
        }

        [Fact]
        public void ClassProxy_Preserves_Behavior_And_Metadata()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ParityClassService>();

            proxy.Name = "class";

            Assert.Equal("class", proxy.Name);
            Assert.Equal("1:payload", proxy.Combine<string>(1, "payload"));

            AssertProxyMetadata(
                typeof(ParityClassService).GetProperty(nameof(ParityClassService.Name)),
                proxy.GetType().GetProperty(nameof(ParityClassService.Name)));
            AssertProxyMetadata(
                typeof(ParityClassService).GetMethod(nameof(ParityClassService.Combine)),
                proxy.GetType().GetMethod(nameof(ParityClassService.Combine)));
        }

        private static void AssertProxyMetadata(PropertyInfo sourceProperty, PropertyInfo proxyProperty)
        {
            Assert.NotNull(sourceProperty);
            Assert.NotNull(proxyProperty);

            Assert.Equal(
                sourceProperty.GetCustomAttribute<DescriptionAttribute>()?.Description,
                proxyProperty.GetCustomAttribute<DescriptionAttribute>()?.Description);
            Assert.NotNull(proxyProperty.GetCustomAttribute<DynamicallyAttribute>());
        }

        private static void AssertProxyMetadata(MethodInfo sourceMethod, MethodInfo proxyMethod)
        {
            Assert.NotNull(sourceMethod);
            Assert.NotNull(proxyMethod);

            Assert.Equal(
                sourceMethod.GetCustomAttribute<DescriptionAttribute>()?.Description,
                proxyMethod.GetCustomAttribute<DescriptionAttribute>()?.Description);
            Assert.NotNull(proxyMethod.GetCustomAttribute<DynamicallyAttribute>());

            var sourceIdParameter = sourceMethod.GetParameters().Single(parameter => parameter.Name == "id");
            var proxyIdParameter = proxyMethod.GetParameters().Single(parameter => parameter.Name == "id");

            Assert.Equal(
                sourceIdParameter.GetCustomAttribute<DescriptionAttribute>()?.Description,
                proxyIdParameter.GetCustomAttribute<DescriptionAttribute>()?.Description);
            Assert.NotNull(proxyIdParameter.GetCustomAttribute<DynamicallyAttribute>());

            var sourcePayloadParameter = sourceMethod.GetParameters().Single(parameter => parameter.Name == "payload");
            var proxyPayloadParameter = proxyMethod.GetParameters().Single(parameter => parameter.Name == "payload");

            Assert.Equal(
                sourcePayloadParameter.GetCustomAttribute<DescriptionAttribute>()?.Description,
                proxyPayloadParameter.GetCustomAttribute<DescriptionAttribute>()?.Description);
            Assert.NotNull(proxyPayloadParameter.GetCustomAttribute<DynamicallyAttribute>());

            Assert.Equal(
                sourceMethod.ReturnParameter.GetCustomAttribute<DescriptionAttribute>()?.Description,
                proxyMethod.ReturnParameter.GetCustomAttribute<DescriptionAttribute>()?.Description);
            Assert.NotNull(proxyMethod.ReturnParameter.GetCustomAttribute<DynamicallyAttribute>());

            var sourceGenericArgument = sourceMethod.GetGenericArguments().Single();
            var proxyGenericArgument = proxyMethod.GetGenericArguments().Single();

            Assert.Equal(
                sourceGenericArgument.GetCustomAttribute<DescriptionAttribute>()?.Description,
                proxyGenericArgument.GetCustomAttribute<DescriptionAttribute>()?.Description);
        }

        public interface IParityService
        {
            [Description(nameof(Name))]
            string Name { get; set; }

            [Description(nameof(Combine))]
            [return: Description(nameof(CombineReturn))]
            string Combine<[Description(nameof(CombineGenericArgument))] TPayload>(
                [Description(nameof(CombineId))] int id,
                [Description(nameof(CombinePayload))] TPayload payload)
                where TPayload : class;
        }

        public class ParityInterfaceService : IParityService
        {
            public string Name { get; set; }

            public string Combine<TPayload>(int id, TPayload payload)
                where TPayload : class
            {
                return $"{id}:{payload}";
            }
        }

        public class ParityClassService
        {
            [Description(nameof(Name))]
            public virtual string Name { get; set; }

            [Description(nameof(Combine))]
            [return: Description(nameof(CombineReturn))]
            public virtual string Combine<[Description(nameof(CombineGenericArgument))] TPayload>(
                [Description(nameof(CombineId))] int id,
                [Description(nameof(CombinePayload))] TPayload payload)
                where TPayload : class
            {
                return $"{id}:{payload}";
            }
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.Interceptors.AddDelegate((ctx, next) => next(ctx));
        }

        private const string CombineReturn = nameof(CombineReturn);
        private const string CombineGenericArgument = nameof(CombineGenericArgument);
        private const string CombineId = nameof(CombineId);
        private const string CombinePayload = nameof(CombinePayload);
    }
}
