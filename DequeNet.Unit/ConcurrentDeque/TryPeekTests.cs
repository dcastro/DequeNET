using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit.ConcurrentDeque
{
    public class TryPeekTests
    {
        [Fact]
        public void TryPeekRight_Inspects_TheRightmostItem()
        {
            var deque = new ConcurrentDeque<int>(new[] {1, 2, 3});

            int item;
            Assert.True(deque.TryPeekRight(out item));
            Assert.Equal(3, item);
            Assert.True(deque.Contains(3));
        }

        [Fact]
        public void TryPeekRight_Fails_IfDequeIsEmpty()
        {
            var deque = new ConcurrentDeque<int>();

            int item;
            Assert.False(deque.TryPeekRight(out item));
        }

        [Fact]
        public void TryPeekLeft_Inspects_TheLeftmostItem()
        {
            var deque = new ConcurrentDeque<int>(new[] {1, 2, 3});

            int item;
            Assert.True(deque.TryPeekLeft(out item));
            Assert.Equal(1, item);
            Assert.True(deque.Contains(1));
        }

        [Fact]
        public void TryPeekLeft_Fails_IfDequeIsEmpty()
        {
            var deque = new ConcurrentDeque<int>();

            int item;
            Assert.False(deque.TryPeekLeft(out item));
        }
    }
}
