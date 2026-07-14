using System.Collections.Generic;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class LinkedListExtensionsTests
    {
        [Fact]
        public void Add_WithNullLinkedList_ThrowsArgumentNullException()
        {
            LinkedList<int> list = null;
            var ex = Assert.Throws<System.ArgumentNullException>(() => list.Add(1));
            Assert.Equal("linkedList", ex.ParamName);
        }

        [Fact]
        public void Add_WithNullValue_ThrowsArgumentNullException()
        {
            var list = new LinkedList<object>();
            var ex = Assert.Throws<System.ArgumentNullException>(() => list.Add(null));
            Assert.Equal("value", ex.ParamName);
        }

        [Fact]
        public void Add_ToEmptyList_AddsFirst()
        {
            var list = new LinkedList<int>();
            var result = list.Add(42);
            Assert.Same(list, result);
            Assert.Single(list);
            Assert.Equal(42, list.First.Value);
        }

        [Fact]
        public void Add_ToNonEmptyList_AddsAfterLast()
        {
            var list = new LinkedList<int>();
            list.AddFirst(1);
            list.Add(2);
            Assert.Equal(2, list.Count);
            Assert.Equal(1, list.First.Value);
            Assert.Equal(2, list.Last.Value);
        }

        [Fact]
        public void Add_MultipleValues_MaintainsOrder()
        {
            var list = new LinkedList<string>();
            list.Add("a");
            list.Add("b");
            list.Add("c");
            Assert.Equal(3, list.Count);
            Assert.Equal(new[] { "a", "b", "c" }, list);
        }
    }
}
