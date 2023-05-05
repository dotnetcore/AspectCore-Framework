using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AspectCore.Extensions.DependencyInjection.ConsoleSample
{
    // https://github.com/dotnetcore/AspectCore-Framework/issues/305
    internal class Issue305
    {
        public static void Start()
        {
            var services = new ServiceCollection();
            services.AddScoped<ITestService, TestService>();
            var serviceProvider = services.BuildServiceContextProvider();
            //            var container = services.ToServiceContext();
            //            container.AddType<ILogger, ConsoleLogger>();
            //            container.AddType<ISampleService, SampleService>();
            //            var serviceResolver = container.Build();
            //            var sampleService = serviceResolver.Resolve<ISampleService>();
            var testService = serviceProvider.GetService<ITestService>();
            testService.Update(("a", "b", "c", "d", "e", "f", "g", "h"));
        }
    }

    public interface ITestService
    {
        //void Update((string, string, string, string, string, string, string) tupleKey);
        void Update((string a, string b, string c, string d, string e, string f, string g, string h) tupleKey);
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
