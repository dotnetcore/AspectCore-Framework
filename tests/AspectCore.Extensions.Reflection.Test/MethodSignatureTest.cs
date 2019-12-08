using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Reflection;

namespace AspectCore.Extensions.Reflection.Test
{
    public class MethodSignatureTest
    {
        [Fact]
        public void Verification()
        {
            Assert.Equal(
                new MethodSignature(typeof(BaseClass).GetMethod("Test1")),
                new MethodSignature(typeof(BaseClass).GetMethod("Test1")));
            Assert.Equal(
                new MethodSignature(typeof(BaseClass).GetMethod("Test2")),
                new MethodSignature(typeof(BaseClass).GetMethod("Test2")));
            Assert.Equal(
                new MethodSignature(typeof(BaseClass).GetMethod("Test3")),
                new MethodSignature(typeof(BaseClass).GetMethod("Test3")));
            Assert.NotEqual(
                new MethodSignature(typeof(BaseClass).GetMethod("Test1")),
                new MethodSignature(typeof(BaseClass).GetMethod("Test2")));

            Assert.Equal(
               new MethodSignature(typeof(BaseClass).GetMethod("Test1")),
               new MethodSignature(typeof(SubClass).GetMethod("Test1")));
            Assert.Equal(
                new MethodSignature(typeof(BaseClass).GetMethod("Test2")),
                new MethodSignature(typeof(SubClass).GetMethod("Test2")));
            Assert.Equal(
                new MethodSignature(typeof(BaseClass).GetMethod("Test3")),
                new MethodSignature(typeof(SubClass).GetMethod("Test3")));
        }
    }

    public class BaseClass
    {
        public virtual string Test1(string value) { return value; }

        public virtual T Test2<T>(T value) { return value; }

        public virtual void Test3<T, V>() { }
    }

    public class SubClass : BaseClass
    {
        public override string Test1(string value)
        {
            return base.Test1(value);
        }

        public override T Test2<T>(T value)
        {
            return base.Test2(value);
        }

        public override void Test3<T, V>()
        {
            base.Test3<T, V>();
        }
    }
}
