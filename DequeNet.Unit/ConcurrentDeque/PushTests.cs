using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit.ConcurrentDeque
{
    public class PushTests
    {
        [Fact]
        public void PushRight_AppendsNode_ToEmptyDeque()
        {
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);

            Assert.Equal(new[] {1}, deque);
        }

        [Fact]
        public void PushRight_AppendsNode_ToNonEmptyList()
        {
            var deque = new ConcurrentDeque<int>(new[] {1, 2, 3});
            deque.PushRight(4);

            Assert.Equal(new[] {1, 2, 3, 4}, deque);
        }

        [Fact]
        public void PushLeft_AppendsNode_ToEmptyDeque()
        {
            var deque = new ConcurrentDeque<int>();
            deque.PushLeft(1);

            Assert.Equal(new[] {1}, deque);
        }

        [Fact]
        public void PushLeft_AppendsNode_ToNonEmptyList()
        {
            var deque = new ConcurrentDeque<int>(new[] {1, 2, 3});
            deque.PushLeft(0);

            Assert.Equal(new[] {0, 1, 2, 3}, deque);
        }
    }
}
