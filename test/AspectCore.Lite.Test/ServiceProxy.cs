using AspectCore.Lite.Abstractions;
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

            IInterceptor t = null;

            joinPoint.AddInterceptor(next =>
            {
                return c =>
                {
                    return t.ExecuteAsync(c , next);
                };
            });

            var @delegate = joinPoint.Build();
            var result = @delegate(context);
        }
    }
}
