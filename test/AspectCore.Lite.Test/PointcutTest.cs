using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Test.Fakes;

namespace AspectCore.Lite.Test
{
    public class PointcutTest : IDependencyInjection
    {
        [Fact]
        public void IsMetchWithNull_Test()
        {
            var provider = this.BuildServiceProvider();
            var pointcut = provider.GetRequiredService<IPointcut>();   
            Assert.False(pointcut.IsMatch(null));
        }

        [Fact]
        public void IsMetchInterface_WithOutInterceptor_Test()
        {
            var provider = this.BuildServiceProvider();
            var pointcut = provider.GetRequiredService<IPointcut>();
            var method = MethodHelper.GetMethodInfo<Action<IServiceWithoutInterceptor>>(x => x.Func());
            Assert.False(pointcut.IsMatch(method));
        }

        [Fact]
        public void IsMetchInterface_WithInterfaceInterceptor_Test()
        {
            var provider = this.BuildServiceProvider();
            var pointcut = provider.GetRequiredService<IPointcut>();
            var method = MethodHelper.GetMethodInfo<Action<IServiceWithInterfaceInterceptor>>(x => x.Func());
            Assert.True(pointcut.IsMatch(method));
        }

        [Fact]
        public void IsMetchInterface_WithMethodInterceptor_Test()
        {
            var provider = this.BuildServiceProvider();
            var pointcut = provider.GetRequiredService<IPointcut>();
            var method = MethodHelper.GetMethodInfo<Action<IServiceWithMethodInterceptor>>(x => x.Func());
            Assert.True(pointcut.IsMatch(method));
        }

        [Fact]
        public void IsMetchClass_WithSealedAndNoInterceptorAndNoVirtual_Test()
        {
            var provider = this.BuildServiceProvider();
            var pointcut = provider.GetRequiredService<IPointcut>();
            var method = MethodHelper.GetMethodInfo<Action<ServiceWithSealedAndNoInterceptorAndNoVirtual>>(x => x.Func());
            Assert.False(pointcut.IsMatch(method));
        }

        [Fact]
        public void IsMetchClass_WithSealedAndInterceptorAndNoVirtual_Test()
        {
            var provider = this.BuildServiceProvider();
            var pointcut = provider.GetRequiredService<IPointcut>();
            var method = MethodHelper.GetMethodInfo<Action<ServiceWithSealedAndInterceptorAndNoVirtual>>(x => x.Func());
            Assert.False(pointcut.IsMatch(method));
        }

        [Fact]
        public void IsMetchClass_WithNoInterceptorAndNoVirtual_Test()
        {
            var provider = this.BuildServiceProvider();
            var pointcut = provider.GetRequiredService<IPointcut>();
            var method = MethodHelper.GetMethodInfo<Action<ServiceWithNoInterceptorAndNoVirtual>>(x => x.Func());
            Assert.False(pointcut.IsMatch(method));
        }

        [Fact]
        public void IsMetchClass_WithInterceptorAndNoVirtual_Test()
        {
            var provider = this.BuildServiceProvider();
            var pointcut = provider.GetRequiredService<IPointcut>();
            var method = MethodHelper.GetMethodInfo<Action<ServiceWithInterceptorAndNoVirtual>>(x => x.Func());
            Assert.False(pointcut.IsMatch(method));
        }

        [Fact]
        public void IsMetchClass_WithInterceptorAndVirtual_Test()
        {
            var provider = this.BuildServiceProvider();
            var pointcut = provider.GetRequiredService<IPointcut>();
            var method = MethodHelper.GetMethodInfo<Action<ServiceWithInterceptorAndVirtual>>(x => x.Func());
            Assert.True(pointcut.IsMatch(method));
        }

        [Fact]
        public void IsMetchClass_WithMethodInterceptorAndVirtual_Test()
        {
            var provider = this.BuildServiceProvider();
            var pointcut = provider.GetRequiredService<IPointcut>();
            var method = MethodHelper.GetMethodInfo<Action<ServiceWithMethodInterceptorAndVirtual>>(x => x.Func());
            Assert.True(pointcut.IsMatch(method));
        }

        public interface IServiceWithoutInterceptor
        {
            void Func();
        }

        [EmptyInterceptor]
        public interface IServiceWithInterfaceInterceptor
        {
            void Func();
        }
   
        public interface IServiceWithMethodInterceptor
        {
            [EmptyInterceptor]
            void Func();
        }

        public sealed class ServiceWithSealedAndNoInterceptorAndNoVirtual
        {
            public void Func()
            {
            }
        }

        [EmptyInterceptor]
        public sealed class ServiceWithSealedAndInterceptorAndNoVirtual
        {
            public void Func()
            {
            }
        }

        public class ServiceWithNoInterceptorAndNoVirtual
        {
            public  void Func()
            {
            }
        }

        [EmptyInterceptor]
        public class ServiceWithInterceptorAndNoVirtual
        {
            public void Func()
            {
            }
        }

        [EmptyInterceptor]
        public class ServiceWithInterceptorAndVirtual
        {
            public virtual void Func()
            {
            }
        }

       
        public class ServiceWithMethodInterceptorAndVirtual
        {
            [EmptyInterceptor]
            public virtual void Func()
            {
            }
        }
    }
}
