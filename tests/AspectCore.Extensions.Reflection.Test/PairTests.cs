using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class PairTests
    {
        private static readonly Assembly ReflectionAssembly = typeof(TypeExtensions).Assembly;

        private static object CreatePair(object item1, object item2)
        {
            var pairType = ReflectionAssembly.GetType("AspectCore.Extensions.Reflection.Pair");
            var createMethod = pairType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .First(m => m.Name == "Create");
            var genericCreate = createMethod.MakeGenericMethod(item1?.GetType() ?? typeof(object), item2?.GetType() ?? typeof(object));
            return genericCreate.Invoke(null, new object[] { item1, item2 });
        }

        private static Type GetPairType(Type t1, Type t2)
        {
            return ReflectionAssembly.GetType("AspectCore.Extensions.Reflection.Pair`2").MakeGenericType(t1, t2);
        }

        [Fact]
        public void Create_Creates_Pair()
        {
            var pair = CreatePair(42, "hello");
            Assert.NotNull(pair);
        }

        [Fact]
        public void Item1_Item2_Properties_Work()
        {
            var pairType = GetPairType(typeof(int), typeof(string));
            var pair = Activator.CreateInstance(pairType, 42, "hello");
            var item1 = pairType.GetProperty("Item1").GetValue(pair);
            var item2 = pairType.GetProperty("Item2").GetValue(pair);
            Assert.Equal(42, item1);
            Assert.Equal("hello", item2);
        }

        [Fact]
        public void Equals_Same_Values_Returns_True()
        {
            var pairType = GetPairType(typeof(int), typeof(string));
            var pair1 = Activator.CreateInstance(pairType, 42, "hello");
            var pair2 = Activator.CreateInstance(pairType, 42, "hello");
            var equalsMethod = pairType.GetMethod("Equals", new Type[] { pairType });
            var result = (bool)equalsMethod.Invoke(pair1, new object[] { pair2 });
            Assert.True(result);
        }

        [Fact]
        public void Equals_Different_Values_Returns_False()
        {
            var pairType = GetPairType(typeof(int), typeof(string));
            var pair1 = Activator.CreateInstance(pairType, 42, "hello");
            var pair2 = Activator.CreateInstance(pairType, 99, "world");
            var equalsMethod = pairType.GetMethod("Equals", new Type[] { pairType });
            var result = (bool)equalsMethod.Invoke(pair1, new object[] { pair2 });
            Assert.False(result);
        }

        [Fact]
        public void Equals_Object_Same_Returns_True()
        {
            var pairType = GetPairType(typeof(int), typeof(string));
            var pair1 = Activator.CreateInstance(pairType, 42, "hello");
            var pair2 = Activator.CreateInstance(pairType, 42, "hello");
            var equalsMethod = pairType.GetMethod("Equals", new Type[] { typeof(object) });
            var result = (bool)equalsMethod.Invoke(pair1, new object[] { pair2 });
            Assert.True(result);
        }

        [Fact]
        public void Equals_Object_Different_Type_Returns_False()
        {
            var pairType = GetPairType(typeof(int), typeof(string));
            var pair1 = Activator.CreateInstance(pairType, 42, "hello");
            var equalsMethod = pairType.GetMethod("Equals", new Type[] { typeof(object) });
            var result = (bool)equalsMethod.Invoke(pair1, new object[] { "not a pair" });
            Assert.False(result);
        }

        [Fact]
        public void GetHashCode_Same_Values_Same_Hash()
        {
            var pairType = GetPairType(typeof(int), typeof(string));
            var pair1 = Activator.CreateInstance(pairType, 42, "hello");
            var pair2 = Activator.CreateInstance(pairType, 42, "hello");
            var hash1 = (int)pairType.GetMethod("GetHashCode").Invoke(pair1, null);
            var hash2 = (int)pairType.GetMethod("GetHashCode").Invoke(pair2, null);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        public void GetHashCode_Null_Items_Does_Not_Throw()
        {
            var pairType = GetPairType(typeof(string), typeof(string));
            var pair = Activator.CreateInstance(pairType, null, null);
            var hash = (int)pairType.GetMethod("GetHashCode").Invoke(pair, null);
            Assert.Equal(0, hash);
        }

        [Fact]
        public void ToString_Returns_Formatted_String()
        {
            var pairType = GetPairType(typeof(int), typeof(string));
            var pair = Activator.CreateInstance(pairType, 42, "hello");
            var result = (string)pairType.GetMethod("ToString").Invoke(pair, null);
            Assert.Equal("(42, hello)", result);
        }

        [Fact]
        public void ToString_Null_Item_Returns_Null_String()
        {
            var pairType = GetPairType(typeof(string), typeof(string));
            var pair = Activator.CreateInstance(pairType, null, "hello");
            var result = (string)pairType.GetMethod("ToString").Invoke(pair, null);
            Assert.Equal("(null, hello)", result);
        }

        [Fact]
        public void Deconstruct_Works()
        {
            var pairType = GetPairType(typeof(int), typeof(string));
            var pair = Activator.CreateInstance(pairType, 42, "hello");
            var deconstructMethod = pairType.GetMethod("Deconstruct");
            var parameters = deconstructMethod.GetParameters();
            Assert.Equal(2, parameters.Length);
        }
    }
}
