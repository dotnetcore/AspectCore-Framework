using AspectCore.Lite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Test
{
    public class ServiceProxy : Object
    {
        private readonly IAspectContextFactoryProvider contextFactoryProvider;
        public void Foo(int age)
        {
            IJoinPoint joinPoint = null;
            var context = contextFactoryProvider.ContextFactory.Create();
            var @delegate = joinPoint.Build();
            @delegate(context);
        }
    }
}
