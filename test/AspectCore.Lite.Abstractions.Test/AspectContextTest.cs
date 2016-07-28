using AspectCore.Lite.Abstractions.Aspects;
using AspectCore.Lite.Abstractions.Test.Fakes;
using Microsoft.AspNetCore.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Lite.Abstractions.Test
{
    public class PointcutTest
    {
        [Fact]
        public void Test()
        {
            AspectDelegate @delegate = (ctx) => Task.FromResult(0);
            ITarget target = new MeaninglessTarget();
            IProxy proxy = new MeaninglessProxy();
            AspectContext context = new MeaninglessAspectContext() { Next = @delegate, Target = target, Proxy = proxy };
            Assert.Equal(@delegate, context.Next);
            Assert.Equal(target, context.Target);
            Assert.Equal(proxy, context.Proxy);
        }
    }
}
