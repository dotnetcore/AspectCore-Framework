using System;
using AspectCore.DependencyInjection;
using AspectCore.Extensions.LightInject;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
    public class ScopeTests
    {
        public class SingletonObj
        {
            public int Age { get; set; }
        }

        public class ScopeObj
        {
            public string Name { get; set; }
        }

        public class TransientObj
        {
            public byte No { get; set; }
        }

        private static IServiceResolver Create()
        {
            var container = new ServiceContainer().RegisterDynamicProxy();
            container.Register<ScopeObj>(new PerScopeLifetime())
                .Register<TransientObj>()
                .Register<SingletonObj>(new PerContainerLifetime());
            return container.GetInstance<IServiceResolver>();
        }

        [Fact]
        public void GetScopeObjOutsideTheScope()
        {
            var provider = Create();
            Assert.Throws<InvalidOperationException>(() => provider.Resolve<ScopeObj>());
        }

        [Fact]
        public void GetScopeObjFromSameScope()
        {
            var provider = Create();
            using (var scope = provider.CreateScope())
            {
                var obj = scope.Resolve<ScopeObj>();
                var obj2 = scope.Resolve<ScopeObj>();
                Assert.Equal(obj, obj2);
            }
        }

        [Fact]
        public void GetScopeObjFromDiffScope()
        {
            var provider = Create();
            using (var outerScope = provider.CreateScope())
            {
                var obj = outerScope.Resolve<ScopeObj>();
                using (var innerScope = provider.CreateScope())
                {
                    var obj2 = innerScope.Resolve<ScopeObj>();
                    Assert.NotEqual(obj, obj2);
                }
            }
        }

        [Fact]
        public void GetTransientObjFromSameScope()
        {
            var provider = Create();
            using (var scope = provider.CreateScope())
            {
                var obj = scope.Resolve<TransientObj>();
                var obj2 = scope.Resolve<TransientObj>();
                Assert.NotEqual(obj, obj2);
            }
        }

        [Fact]
        public void GetTransientObjFromDiffScope()
        {
            var provider = Create();
            using (var outerScope = provider.CreateScope())
            {
                var obj = outerScope.Resolve<TransientObj>();
                using (var innerScope = provider.CreateScope())
                {
                    var obj2 = innerScope.Resolve<TransientObj>();
                    Assert.NotEqual(obj, obj2);
                }
            }
        }

        [Fact]
        public void GetSingletonObjFromSameScope()
        {
            var provider = Create();
            using (var scope = provider.CreateScope())
            {
                var obj = scope.Resolve<SingletonObj>();
                var obj2 = scope.Resolve<SingletonObj>();
                Assert.Equal(obj, obj2);
            }
        }

        [Fact]
        public void GetSingletonObjFromDiffScope()
        {
            var provider = Create();
            using (var outerScope = provider.CreateScope())
            {
                var obj = outerScope.Resolve<SingletonObj>();
                using (var innerScope = provider.CreateScope())
                {
                    var obj2 = innerScope.Resolve<SingletonObj>();
                    Assert.Equal(obj, obj2);
                }
            }
        }
    }
}
