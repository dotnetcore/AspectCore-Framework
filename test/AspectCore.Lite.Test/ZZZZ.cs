using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.Test
{
    public class ZZZZ : IZZZZ
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IZZZZ zzz;


        string IZZZZ.Name
        {
            get; set;
        }

        public Task Foo()
        {
            var aspectExector = serviceProvider.GetRequiredService<IAspectExecutor>();
            return aspectExector.ExecuteAsync<object>(zzz, this, typeof(IZZZZ), "Foo");
        }

        public Task<string> Foo1()
        {
            var aspectExector = serviceProvider.GetRequiredService<IAspectExecutor>();
            return aspectExector.ExecuteAsync<string>(zzz, this, typeof(IZZZZ), "Foo1");
        }

        public virtual void FooS()
        {
            var aspectExector = serviceProvider.GetRequiredService<IAspectExecutor>();
            aspectExector.ExecuteSynchronously<object>(zzz, this, typeof(IZZZZ), "FooS");
        }

        public string FooS1()
        {
            var aspectExector = serviceProvider.GetRequiredService<IAspectExecutor>();
            return aspectExector.ExecuteSynchronously<string>(zzz, this, typeof(IZZZZ), "FooS1");
        }
    }


    public class ZZZZZ2 : ZZZZ
    {
        public override void FooS()
        {
            base.FooS();
        }



        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    public interface IZZZZ
    {

        string Name { get; set; }

        Task Foo();

        Task<String> Foo1();

        void FooS();

        string FooS1();
    }
}
