using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit.Deque
{
    public class ConstructorTests
    {
        [Fact]
        public void WithNegativeCapacity_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Deque<int>(-1));
        }

        [Fact]
        public void WithZeroCapacity_HasZeroCapacity()
        {
            var deque = new Deque<int>(0);
            Assert.Equal(0, deque.Capacity);
        }

        [Fact]
        public void WithCapacity_HasSpecifiedCapacity()
        {
            var deque = new Deque<int>(5);
            Assert.Equal(5, deque.Capacity);
        }

        [Fact]
        public void WithICollection_CopiesCollection()
        {
            var array = new[] {1, 2, 3};
            var deque = new Deque<int>(array);

            Assert.Equal(array, deque);
            Assert.Equal(array.Length, deque.Capacity);
        }

        [Fact]
        public void WithIEnumerable_CopiesCollection()
        {
            var queue = new Queue<int>(new[] {1, 2, 3});
            var deque = new Deque<int>(queue);

            Assert.Equal(queue, deque);
            Assert.Equal(queue.Count, deque.Capacity);
        }

        [Fact]
        public void HasZeroCapacityByDefault()
        {
            var deque = new Deque<int>();
            Assert.Equal(0, deque.Capacity);
        }
    }
}
