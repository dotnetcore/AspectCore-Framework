using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Tests.DependencyInjection
{
    public class OptionalArgumentsTests
    {
        public class Dependency { }
        public class OptionalDependency { }

        public class Service
        {
            private readonly Dependency _dependency;
            private readonly OptionalDependency _optionalDependency;

            public Service(Dependency dependency, OptionalDependency optionalDependency = null)
            {
                _dependency = dependency;
                _optionalDependency = optionalDependency;
            }
            public bool HasDependency => _dependency != null;
            public bool HasOptionalDependency => _optionalDependency != null;
        }

        [Fact]
        public void Resolve_OptionalDependencyExists()
        {
            var serviceContext = new ServiceContext();
            serviceContext
                .AddType<Dependency>()
                .AddType<OptionalDependency>()
                .AddType<Service>();

            var container = serviceContext.Build();
            var service = container.Resolve<Service>();
            Assert.True(service.HasDependency);
            Assert.True(service.HasOptionalDependency);
        }

        [Fact]
        public void Resolve_OptionalDependencyDoesNotExists()
        {
            var serviceContext = new ServiceContext();
            serviceContext
                .AddType<Dependency>()
                .AddType<Service>();

            var container = serviceContext.Build();
            var service = container.Resolve<Service>();
            Assert.True(service.HasDependency);
            Assert.False(service.HasOptionalDependency);
        }
    }
}
