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


        public void Foo(string name)
        {
            //zzz.Foo();
            var aspectExector = serviceProvider.GetRequiredService<IAspectExecutor>();
            aspectExector.ExecuteSynchronously(zzz , this , typeof(IZZZZ) , "Foo" , name);
        }
    }


    public interface IZZZZ
    {
        void Foo(string name);
    }
}
