using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test.Issues
{
    // https://github.com/dotnetcore/AspectCore-Framework/issues/305
    public class ValueTupleMorethan7ElementsTests
    {
        [Fact]
        public void ValueTupleMorethan7Elements_Constructor_Test()
        {
            var services = new ServiceCollection();
            services.AddScoped<ITestService, TestService>();
            var serviceProvider = services.BuildServiceContextProvider();
            var testService = serviceProvider.GetService<ITestService>();
            Assert.NotNull(testService);

            var (a, b, c, d, e, f, g, h) = testService.Wrap(("a", "b", "c", "d", "e", "f", "g", "h"));
            Assert.Equal("a", a);
            Assert.Equal("b", b);
            Assert.Equal("c", c);
            Assert.Equal("d", d);
            Assert.Equal("e", e);
            Assert.Equal("f", f);
            Assert.Equal("g", g);
            Assert.Equal("h", h);
        }
        public interface ITestService
        {
            //void Update((string, string, string, string, string, string, string) tupleKey);
            void Update((string a, string b, string c, string d, string e, string f, string g, string h) tupleKey);
            (string a, string b, string c, string d, string e, string f, string g, string h) Wrap((string a, string b, string c, string d, string e, string f, string g, string h) tupleKey);
            //void Update2((string a, string b, string c, string d, string e, string f, string g) tupleKey);
            //void Update3((string a, string b, string c, string d, string e, string f, string g, string h, (string i, string j)) tupleKey);
        }

        public class TestService : ITestService
        {
            //public void Update((string, string, string, string, string, string, string) tupleKey)
            //{

            //}
            public void Update((string a, string b, string c, string d, string e, string f, string g, string h) tupleKey)
            {
            }

            public (string a, string b, string c, string d, string e, string f, string g, string h) Wrap((string a, string b, string c, string d, string e, string f, string g, string h) tupleKey)
            {
                return tupleKey;
            }

            //public void Update2((string a, string b, string c, string d, string e, string f, string g) tupleKey)
            //{

            //}

            //public void Update3((string a, string b, string c, string d, string e, string f, string g, string h, (string i, string j)) tupleKey)
            //{

            //}

            //public void Update6((string a, string b, string c, string d, string e, string f, string g, (string h, string i, string j) ff) tupleKey)
            //{

            //}

            //public void Update4((string, string, string, string, string, string, string) tupleKey)
            //{

            //}

            //public void Update5((string, string, string, string, string, string, string, string, string) tupleKey)
            //{

            //}
        }
    }
}
