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
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new MethodFakes(), "lemon");
            Assert.Equal("lemon", result);
        }

        [Fact]
        public void Invoker_Static_Test()
        {
            var method = typeof(MethodFakes).GetMethod("GetStringStatic");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(null, "lemon");
            Assert.Equal("lemon", result);
            result = reflector.StaticInvoke("lemon");
            Assert.Equal("lemon", result);
        }

        [Fact]
        public void Invoker_Call_Test()
        {
            var method = typeof(MethodFakes).GetMethod("GetString");

            var reflector = method.GetReflector(CallOptions.Callvirt);
            var result = reflector.Invoke(new SubMethodFakes(), "lemon");
            Assert.Equal(typeof(string).Name, result);

            reflector = method.GetReflector(CallOptions.Call);
            result = reflector.Invoke(new SubMethodFakes(), "lemon");
            Assert.Equal("lemon", result);
        }

        [Fact]
        public void Invoker_Ref_Test()
        {
            var method = typeof(MethodFakes).GetMethod("GetStringByRef");
            var args = new object[] { "lemon", null };
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new MethodFakes(), args);
            Assert.Equal("lemon", args[1]);
        }

        [Fact]
        public void Struct_Invoker_Test()
        {
            var method = typeof(StructMethodFakes).GetMethod("GetString");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new StructMethodFakes(), "lemon");
            Assert.Equal("lemon", result);
        }

        [Fact]
        public void _Return_Struct_Invoker_Test()
        {
            var method = typeof(MethodFakes).GetMethod("GetValue");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new MethodFakes());
            Assert.Equal(1, result);
        }

        [Fact]
        public void Method_DisplayName()
        {
            var method = typeof(MethodFakes).GetMethod("GetString");
            var reflector = method.GetReflector();
            var displayName = reflector.DisplayName;
            Assert.Equal("String GetString(Object)", displayName);
        }

        [Fact]
        public void Out_Method_DisplayName()
        {
            var method = typeof(MethodFakes).GetMethod("GetStringByRef");
            var reflector = method.GetReflector();
            var displayName = reflector.DisplayName;
            Assert.Equal("Void GetStringByRef(Object,String&)", displayName);
        }

        [Fact]
        public void OpenGeneric_Method_DisplayName()
        {
            var method = typeof(MethodFakes).GetMethod("GetValue2");
            var reflector = method.GetReflector();
            var displayName = reflector.DisplayName;
            Assert.Equal("T GetValue2<T>()", displayName);

            method = typeof(MethodFakes<>).GetMethod("GetString");
            reflector = method.GetReflector();
            displayName = reflector.DisplayName;
            Assert.Equal("String GetString(T)", displayName);
        }

        [Fact]
        public void CloseGeneric_Method_DisplayName()
        {
            var method = typeof(MethodFakes).GetMethod("GetValue2").MakeGenericMethod(typeof(string));
            var reflector = method.GetReflector();
            var displayName = reflector.DisplayName;
            Assert.Equal("String GetValue2<String>()", displayName);
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

        public int GetValue()
        {
            return 1;
        }

        public T GetValue2<T>()
        {
            return default(T);
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

    public struct StructMethodFakes
    {
        public string GetString(object value)
        {
            return value.ToString();
        }
    }
}
