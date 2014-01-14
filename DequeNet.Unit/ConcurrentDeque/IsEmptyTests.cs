using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit.ConcurrentDeque
{
    public class IsEmptyTests
    {
        [Fact]
        public void IsEmptyReturnsTrueIfDequeIsEmpty()
        {
            var deque = new ConcurrentDeque<int>();

            Assert.True(deque.IsEmpty);
        }

        [Fact]
        public void IsEmptyReturnsFalseIfDequeHasItems()
        {
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(0);

            Assert.False(deque.IsEmpty);
        }
    }
}
