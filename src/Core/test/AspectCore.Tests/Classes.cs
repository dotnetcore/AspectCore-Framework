using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Injector;

namespace AspectCore.Tests
{
    public interface IService
    {
        Guid Id { get; }

        ILogger Logger { get; set; }
    }

    public class ServiceBase : IService
    {
        public Guid Id { get; } = Guid.NewGuid();
        public virtual ILogger Logger { get; set; }
    }

    public interface ITransient : IService { }

    public class Transient : ServiceBase, ITransient, IDelegateTransient { }

    public interface IDelegateTransient : ITransient { }

    public interface IScoped : IService, IDisposable { bool IsDisposed { get; } }

    public class Scoped : ServiceBase, IScoped
    {
        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    public interface ISingleton : IScoped { }

    public class Singleton : Scoped, ISingleton { }

    public interface ILogger { }

    public class Logger : ILogger { }

    public class PropertyInjectionService : ServiceBase
    {
        [FromContainer]
        public override ILogger Logger { get; set; }

        [FromContainer]
        internal ILogger InternalLogger { get; set; }
    }

    public interface ISimpleGeneric<T> { }

    public class SimpleGeneric<T> : ISimpleGeneric<T>, IInstanceSimpleGeneric<T>, IDelegateSimpleGeneric<T> { }

    public interface IInstanceSimpleGeneric<T> : ISimpleGeneric<T> { }

    public interface IDelegateSimpleGeneric<T> : ISimpleGeneric<T> { }

}