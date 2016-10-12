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


        public void Foo(string name ,int id)
        {
            //zzz.Foo();
            var aspectExector = serviceProvider.GetRequiredService<IAspectExecutor>();
            aspectExector.ExecuteSynchronously(zzz, this, typeof(IZZZZ), "Foo", name, id);
        }

        public void Foo1(ref int id)
        {
            var aspectExector = serviceProvider.GetRequiredService<IAspectExecutor>();
            aspectExector.ExecuteSynchronously(zzz, this, typeof(IZZZZ), "Foo1", id);
        }
    }


    public interface IZZZZ
    {
        void Foo(string name, int id);

        void Foo1(ref int id);
    }
}
