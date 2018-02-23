using System;
using System.Collections.Generic;
using System.Text;

namespace Extensions.Hosting.Tests
{
    public interface IService
    {
        string GetValue();
    }

    public class Service : IService
    {
        public string GetValue()
        {
            return "service";
        }
    }
}
