using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class MethodReflectorTests
    {
        [Fact]
        public void Invoker_Instance_Test()
        {
            var method = typeof(MethodFakes).GetMethod("GetString");
            var reflector = method.AsReflector();
            var result = reflector.Invoke(new MethodFakes(), "lemon");
            Assert.Equal("lemon", result);
        }

        [Fact]
        public void Invoker_Static_Test()
        {
            var method = typeof(MethodFakes).GetMethod("GetStringStatic");
            var reflector = method.AsReflector();
            var result = reflector.Invoke(null, "lemon");
            Assert.Equal("lemon", result);
            result = reflector.StaticInvoke("lemon");
            Assert.Equal("lemon", result);
        }

        [Fact]
        public  void Invoker_Call_Test()
        {
            var method = typeof(MethodFakes).GetMethod("GetString");

            var reflector = method.AsReflector(CallOptions.Callvirt);
            var result = reflector.Invoke(new SubMethodFakes(), "lemon");
            Assert.Equal(typeof(string).Name, result);

            reflector = method.AsReflector(CallOptions.Call);
            result = reflector.Invoke(new SubMethodFakes(), "lemon");
            Assert.Equal("lemon", result);
        }

        [Fact]
        public void Invoker_Ref_Test()
        {
            var method = typeof(MethodFakes).GetMethod("GetStringByRef");
            var args = new object[] { "lemon", null };
            var reflector = method.AsReflector();
            var result = reflector.Invoke(new MethodFakes(), args);
            Assert.Equal("lemon", args[1]);
        }
    }

    public class MethodFakes
    {
        public virtual string GetString(object value)
        {
            return value.ToString();
        }

        public static string GetStringStatic(object value)
        {
            return value.ToString();
        }

        public void GetStringByRef(object value, out string @string)
        {
            @string = value.ToString();
        }
    }

    public class SubMethodFakes : MethodFakes
    {
        public override string GetString(object value)
        {
            return value.GetType().Name;
        }
    }

    public class MethodFakes<T>
    {
        public virtual string GetString(T value)
        {
            return value.ToString();
        }
    }
}
