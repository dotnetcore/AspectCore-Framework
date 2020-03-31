using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Tests
{
    public interface IService
    {
        Guid Id { get; set; }

        ILogger Logger { get; set; }
    }

    public class ServiceBase : IService
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [FromServiceContext]
        public virtual ILogger Logger { get; set; }
    }

    public interface ITransient : IService
    {
        void Foo();
    }

    public class Transient : ServiceBase, ITransient, IDelegateTransient
    {
        public virtual void Foo()
        {
        }
    }

    public interface IDelegateTransient : ITransient
    {
    }

    public interface IScoped : IService, IDisposable
    {
        bool IsDisposed { get; }
    }

    public class Scoped : ServiceBase, IScoped
    {
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public interface ISingleton : IScoped
    {
    }

    public class Singleton : Scoped, ISingleton
    {
    }

    public interface ILogger
    {
        void Info();
    }

    public class Logger : ILogger
    {
        public void Info()
        {
        }
    }

    public interface IPropertyInjectionService
    {

    }

    public class PropertyInjectionService : ServiceBase, IPropertyInjectionService
    {
        [FromServiceContext]
        internal ILogger InternalLogger { get; set; }
    }

    public interface ISimpleGeneric<T>
    {
    }

    public class SimpleGeneric<T> : ISimpleGeneric<T>, IInstanceSimpleGeneric<T>, IDelegateSimpleGeneric<T>
    {
    }

    public interface IInstanceSimpleGeneric<T> : ISimpleGeneric<T>
    {
    }

    public interface IDelegateSimpleGeneric<T> : ISimpleGeneric<T>
    {
    }

    public interface IUserService : IService
    {
        IRepository<User> Repository { get; }
    }

    public class UserService : ServiceBase, IUserService
    {
        public UserService(IRepository<User> repository)
        {
            Repository = repository;
        }

        public IRepository<User> Repository { get; }
    }

    public interface IRepository<T>
    {
    }

    public class Repository<T> : IRepository<T>
    {
    }

    public class User
    {
    }

    public class PocoClass
    {
    }

    public interface IFakeOpenGenericService<TValue>
    {
        TValue Value { get; }
    }

    public class FakeService : IFakeOpenGenericService<PocoClass>
    {
        public PocoClass Value { get; set; }
    }

    public class FakeOpenGenericService<TVal> : IFakeOpenGenericService<TVal>
    {
        public FakeOpenGenericService(TVal value)
        {
            Value = value;
        }

        public TVal Value { get; }
    }

    public class AbsFakeOpenGenericMethod
    {
        public virtual T Create<T>() where T : class
        {
            return default(T);
        }
    }

    public class FakeOpenGenericMethod : AbsFakeOpenGenericMethod
    {
        public override T Create<T>()
        {
            return base.Create<T>();
        }
    }

    public class FakeAsyncClass
    {
        [DynAsyncTestInterceptor]
        public virtual dynamic DynAsync(int value)
        {
            return Task.Run<int>(async () =>
            {
                await Task.Delay(value);
                return value;
            });
        }

        [AsyncTestInterceptor]
        public virtual Task<int> Async(int value)
        {
            return Task.Run<int>(async () =>
            {
                await Task.Delay(value);
                return value;
            });
        }
    }

    public class AsyncTestInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            Assert.True(context.IsAsync());
            await context.Invoke(next);
            var result = await context.UnwrapAsyncReturnValue();
            Assert.Equal(100, result);
        }
    }

    public class DynAsyncTestInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            Assert.False(context.IsAsync());
            await context.Invoke(next);
            Assert.True(context.IsAsync());
            var result = await context.UnwrapAsyncReturnValue();
            Assert.Equal(100, result);
        }
    }

    public class FakeProperty
    {
        public string Val { get; set; }
    }

    public interface IFakeExplicitImplementation
    {
        string GetVal();

        string GetVal_NonAspect();

        int GetVal2();
    }

    public class FakeExplicitImplementation : IFakeExplicitImplementation
    {
        string IFakeExplicitImplementation.GetVal()
        {
            return "lemon";
        }

        int IFakeExplicitImplementation.GetVal2()
        {
            return 1;
        }

        string IFakeExplicitImplementation.GetVal_NonAspect()
        {
            return "lemon";
        }
    }
}