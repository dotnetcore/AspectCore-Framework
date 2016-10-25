using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Lite.Abstractions;
using System.Reflection;
using Microsoft.AspNetCore.Testing;

namespace AspectCore.Lite.Test
{
    public class NamedMethodMatcherTest : IDependencyInjection
    {
        [Fact]
        public void MatchWithNoParameter_Test()
        {
            var provider = this.BuildServiceProvider();
            var namedMethodMatcher = provider.GetRequiredService<INamedMethodMatcher>();
            var method = namedMethodMatcher.Match(typeof(INamedMethodMatcherTestService), "Func");
            Assert.Equal(method, typeof(INamedMethodMatcherTestService).GetTypeInfo().GetMethod("Func", Type.EmptyTypes));
        }

        [Fact]
        public void MatchWithParameter_Test()
        {
            var provider = this.BuildServiceProvider();
            var namedMethodMatcher = provider.GetRequiredService<INamedMethodMatcher>();
            var method = namedMethodMatcher.Match(typeof(INamedMethodMatcherTestService), "Func", 0);
            Assert.Equal(method, typeof(INamedMethodMatcherTestService).GetTypeInfo().GetMethod("Func", new Type[] { typeof(int) }));
            method = namedMethodMatcher.Match(typeof(INamedMethodMatcherTestService), "Func", "test", new object());
            Assert.Equal(method, typeof(INamedMethodMatcherTestService).GetTypeInfo().GetMethod("Func", new Type[] { typeof(string), typeof(object) }));
        }

        [Fact]
        public void Match_ThrowsInvalidOperationExceptionException()
        {
            ExceptionAssert.Throws<InvalidOperationException>(()=> {
                var provider = this.BuildServiceProvider();
                var namedMethodMatcher = provider.GetRequiredService<INamedMethodMatcher>();
                var method = namedMethodMatcher.Match(typeof(INamedMethodMatcherTestService), "Func", new object());
            },
            $"A suitable method for type '{typeof(INamedMethodMatcherTestService)}' could not be located. Ensure the type is concrete and services are registered for all parameters of a public method.");
        }

        public interface INamedMethodMatcherTestService
        {
            void Func();

            void Func(int id);

            void Func(string n, object obj);
        }
    }
}
