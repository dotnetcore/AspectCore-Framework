using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Lite.Abstractions;
#if NETCOREAPP1_1
using Microsoft.AspNetCore.Testing;
#endif

namespace AspectCore.Lite.Test
{
    public class NamedMethodMatcherTest : IDependencyInjection
    {
        [Fact]
        public void MatchWithNoParameter_Test()
        {
            var provider = this.BuildServiceProvider();
            var namedMethodMatcher = provider.GetRequiredService<INamedMethodMatcher>();
            var method = namedMethodMatcher.Match(typeof(INamedMethodMatcherTestService) , "Func");
            Assert.Equal(method , MethodHelper.GetMethodInfo<Action<INamedMethodMatcherTestService>>((s) => s.Func()));
        }

        [Fact]
        public void MatchWithParameter_Test()
        {
            var provider = this.BuildServiceProvider();
            var namedMethodMatcher = provider.GetRequiredService<INamedMethodMatcher>();
            var method = namedMethodMatcher.Match(typeof(INamedMethodMatcherTestService) , "Func" , 0);
            Assert.Equal(method , MethodHelper.GetMethodInfo<Action<INamedMethodMatcherTestService , int>>((s , id) => s.Func(id)));
            method = namedMethodMatcher.Match(typeof(INamedMethodMatcherTestService) , "Func" , "test" , new object());
            Assert.Equal(method , MethodHelper.GetMethodInfo<Action<INamedMethodMatcherTestService , string , object>>((s , n , obj) => s.Func(n , obj)));
        }

#if NETCOREAPP1_1
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
#endif
        public interface INamedMethodMatcherTestService
        {
            void Func();

            void Func(int id);

            void Func(string n , object obj);
        }
    }
}
