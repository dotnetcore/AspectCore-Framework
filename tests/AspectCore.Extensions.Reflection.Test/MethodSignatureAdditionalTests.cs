using System;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class MethodSignatureAdditionalTests
    {
        [Fact]
        public void Constructor_Null_Method_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MethodSignature((MethodBase)null));
        }

        [Fact]
        public void Constructor_With_Method_Creates_Signature()
        {
            var method = typeof(SignatureFakes).GetMethod("Add");
            var signature = new MethodSignature(method);
            Assert.Equal("Add", signature.Name);
        }

        [Fact]
        public void Constructor_With_Method_And_Name_Creates_Signature()
        {
            var method = typeof(SignatureFakes).GetMethod("Add");
            var signature = new MethodSignature(method, "CustomName");
            Assert.Equal("CustomName", signature.Name);
        }

        [Fact]
        public void Equals_Same_Signature_Returns_True()
        {
            var method1 = typeof(SignatureFakes).GetMethod("Add");
            var method2 = typeof(SignatureFakes).GetMethod("Add");
            var sig1 = new MethodSignature(method1);
            var sig2 = new MethodSignature(method2);
            Assert.True(sig1.Equals(sig2));
            Assert.True(sig1 == sig2);
            Assert.False(sig1 != sig2);
        }

        [Fact]
        public void Equals_Different_Signature_Returns_False()
        {
            var method1 = typeof(SignatureFakes).GetMethod("Add");
            var method2 = typeof(SignatureFakes).GetMethod("Subtract");
            var sig1 = new MethodSignature(method1);
            var sig2 = new MethodSignature(method2);
            Assert.False(sig1.Equals(sig2));
            Assert.False(sig1 == sig2);
            Assert.True(sig1 != sig2);
        }

        [Fact]
        public void Equals_Object_Not_Signature_Returns_False()
        {
            var method = typeof(SignatureFakes).GetMethod("Add");
            var sig = new MethodSignature(method);
            Assert.False(sig.Equals("not a signature"));
        }

        [Fact]
        public void Equals_Object_Null_Returns_False()
        {
            var method = typeof(SignatureFakes).GetMethod("Add");
            var sig = new MethodSignature(method);
            Assert.False(sig.Equals(null));
        }

        [Fact]
        public void GetHashCode_Same_Signature_Same_Hash()
        {
            var method1 = typeof(SignatureFakes).GetMethod("Add");
            var method2 = typeof(SignatureFakes).GetMethod("Add");
            var sig1 = new MethodSignature(method1);
            var sig2 = new MethodSignature(method2);
            Assert.Equal(sig1.GetHashCode(), sig2.GetHashCode());
        }

        [Fact]
        public void Value_Property_Returns_Signature_Code()
        {
            var method = typeof(SignatureFakes).GetMethod("Add");
            var sig = new MethodSignature(method);
            Assert.NotEqual(0, sig.Value);
        }

        [Fact]
        public void Different_Parameter_Count_Different_Signature()
        {
            var method1 = typeof(SignatureFakes).GetMethod("Add");
            var method2 = typeof(SignatureFakes).GetMethod("AddOne");
            var sig1 = new MethodSignature(method1);
            var sig2 = new MethodSignature(method2);
            Assert.False(sig1.Equals(sig2));
        }

        [Fact]
        public void Generic_Method_Signature_Works()
        {
            var method = typeof(SignatureFakes).GetMethod("GenericMethod");
            var sig = new MethodSignature(method);
            Assert.Equal("GenericMethod", sig.Name);
        }

        [Fact]
        public void Method_With_Generic_Parameter_Signature_Works()
        {
            var method = typeof(SignatureFakes).GetMethod("GenericParam");
            var sig = new MethodSignature(method);
            Assert.Equal("GenericParam", sig.Name);
        }
    }

    public class SignatureFakes
    {
        public int Add(int a, int b) => a + b;
        public int Subtract(int a, int b) => a - b;
        public int AddOne(int a) => a + 1;
        public T GenericMethod<T>() => default(T);
        public void GenericParam<T>(T value) { }
    }
}
