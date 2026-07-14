using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class MethodReflectorAdditionalTests
    {
        #region Void Return Type

        [Fact]
        public void Invoke_Void_Method_Works()
        {
            var method = typeof(MethodFakes).GetMethod("VoidMethod");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new MethodFakes());
            Assert.Null(result);
        }

        [Fact]
        public void StaticInvoke_Void_Method_Works()
        {
            var method = typeof(MethodFakes).GetMethod("StaticVoidMethod");
            var reflector = method.GetReflector();
            var result = reflector.StaticInvoke();
            Assert.Null(result);
        }

        [Fact]
        public void Invoke_Void_Method_With_Params_Works()
        {
            var method = typeof(MethodFakes).GetMethod("VoidMethodWithParams");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new MethodFakes(), 42, "hello");
            Assert.Null(result);
        }

        #endregion

        #region Multiple Parameters

        [Fact]
        public void Invoke_Multiple_Parameters_Works()
        {
            var method = typeof(MethodFakes).GetMethod("Add");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new MethodFakes(), 3, 4);
            Assert.Equal(7, result);
        }

        [Fact]
        public void StaticInvoke_Multiple_Parameters_Works()
        {
            var method = typeof(MethodFakes).GetMethod("StaticAdd");
            var reflector = method.GetReflector();
            var result = reflector.StaticInvoke(3, 4);
            Assert.Equal(7, result);
        }

        [Fact]
        public void Invoke_Multiple_Parameters_With_Different_Types_Works()
        {
            var method = typeof(MethodFakes).GetMethod("Concat");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new MethodFakes(), "hello", 42);
            Assert.Equal("hello42", result);
        }

        #endregion

        #region Ref/Out Parameters

        [Fact]
        public void Invoke_Ref_Parameter_Works()
        {
            var method = typeof(MethodFakes).GetMethod("ModifyRef");
            var reflector = method.GetReflector();
            var args = new object[] { 10 };
            var result = reflector.Invoke(new MethodFakes(), args);
            Assert.Equal(20, args[0]);
        }

        [Fact]
        public void Invoke_Out_Parameter_Works()
        {
            var method = typeof(MethodFakes).GetMethod("ModifyOut");
            var reflector = method.GetReflector();
            var args = new object[] { null };
            var result = reflector.Invoke(new MethodFakes(), args);
            Assert.Equal("output", args[0]);
        }

        [Fact]
        public void Invoke_Multiple_Ref_Parameters_Works()
        {
            var method = typeof(MethodFakes).GetMethod("ModifyMultipleRef");
            var reflector = method.GetReflector();
            var args = new object[] { 10, 20 };
            var result = reflector.Invoke(new MethodFakes(), args);
            Assert.Equal(20, args[0]);
            Assert.Equal(40, args[1]);
        }

        [Fact]
        public void Invoke_Ref_And_Out_Parameters_Works()
        {
            var method = typeof(MethodFakes).GetMethod("ModifyRefAndOut");
            var reflector = method.GetReflector();
            var args = new object[] { 10, null };
            var result = reflector.Invoke(new MethodFakes(), args);
            Assert.Equal(20, args[0]);
            Assert.Equal("result", args[1]);
        }

        [Fact]
        public void StaticInvoke_Ref_Parameter_Works()
        {
            var method = typeof(MethodFakes).GetMethod("StaticModifyRef");
            var reflector = method.GetReflector();
            var args = new object[] { 10 };
            var result = reflector.StaticInvoke(args);
            Assert.Equal(30, args[0]);
        }

        [Fact]
        public void StaticInvoke_Out_Parameter_Works()
        {
            var method = typeof(MethodFakes).GetMethod("StaticModifyOut");
            var reflector = method.GetReflector();
            var args = new object[] { null };
            var result = reflector.StaticInvoke(args);
            Assert.Equal("static_output", args[0]);
        }

        #endregion

        #region Value Type Return

        [Fact]
        public void Invoke_ValueType_Return_Works()
        {
            var method = typeof(MethodFakes).GetMethod("GetInt");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new MethodFakes());
            Assert.Equal(42, result);
        }

        [Fact]
        public void StaticInvoke_ValueType_Return_Works()
        {
            var method = typeof(MethodFakes).GetMethod("StaticGetInt");
            var reflector = method.GetReflector();
            var result = reflector.StaticInvoke();
            Assert.Equal(99, result);
        }

        #endregion

        #region No Parameters

        [Fact]
        public void Invoke_No_Parameters_Works()
        {
            var method = typeof(MethodFakes).GetMethod("GetInt");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new MethodFakes());
            Assert.Equal(42, result);
        }

        [Fact]
        public void StaticInvoke_No_Parameters_Works()
        {
            var method = typeof(MethodFakes).GetMethod("StaticGetInt");
            var reflector = method.GetReflector();
            var result = reflector.StaticInvoke();
            Assert.Equal(99, result);
        }

        #endregion

        #region Struct Method Invocation

        [Fact]
        public void Invoke_Struct_Method_With_Void_Works()
        {
            var method = typeof(StructMethodFakes).GetMethod("VoidMethod");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new StructMethodFakes());
            Assert.Null(result);
        }

        [Fact]
        public void Invoke_Struct_Method_With_Multiple_Params_Works()
        {
            var method = typeof(StructMethodFakes).GetMethod("Add");
            var reflector = method.GetReflector();
            var result = reflector.Invoke(new StructMethodFakes(), 3, 4);
            Assert.Equal(7, result);
        }

        [Fact]
        public void Invoke_Struct_Method_With_Ref_Param_Works()
        {
            var method = typeof(StructMethodFakes).GetMethod("ModifyRef");
            var reflector = method.GetReflector();
            var args = new object[] { 10 };
            var result = reflector.Invoke(new StructMethodFakes(), args);
            Assert.Equal(20, args[0]);
        }

        #endregion
    }
}
