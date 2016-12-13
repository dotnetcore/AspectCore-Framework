using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Test
{
    public class DescriptorWithParameter
    {
        public static readonly MethodInfo Method = typeof(DescriptorWithParameter).GetTypeInfo().DeclaredMethods.Where(m => m.Name == "TestMethodWithParameter").First();
        public static readonly MethodInfo InvokeMethod = typeof(DescriptorWithParameter).GetTypeInfo().DeclaredMethods.Where(m => m.Name == "Invoke").First();

        public static readonly ParameterInfo[] Parameters = Method.GetParameters();

        public static readonly ParameterInfo ReturnParameter = Method.ReturnParameter;

        public void TestMethodWithParameter(int age, string name, object obj)
        {
        }

        public object Invoke(object obj)
        {
            return obj;
        }
    }
}
