using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit.Deque
{
    public class PeekTests
    {
        [Fact]
        public void PeekLeft_ThrowsException_WhenEmpty()
        {
            var deque = new Deque<int>();
            Assert.Throws<InvalidOperationException>(() => deque.PeekLeft());
        }

        [Fact]
        public void PeekLeft_ReturnsLeftmostItem()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            var item = deque.PeekLeft();

            Assert.Equal(1, item);
        }

        [Fact]
        public void PeekLeft_DoesntRemoveItem()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            deque.PeekLeft();

            Assert.Equal(new[] {1, 2, 3}, deque);
        }

        [Fact]
        public void PeekRight_ThrowsException_WhenEmpty()
        {
            var deque = new Deque<int>();
            Assert.Throws<InvalidOperationException>(() => deque.PeekRight());
        }

        [Fact]
        public void PeekRight_ReturnsRightmostItem()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            var item = deque.PeekRight();

            Assert.Equal(3, item);
        }

        [Fact]
        public void PeekRight_ReturnsRightmostItem_WhenDequeLoopsAround()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            deque.PopLeft();
            deque.PushRight(4);

            var item = deque.PeekRight();

            Assert.Equal(4, item);
        }

        [Fact]
        public void PeekRight_DoesntRemoveItem()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            deque.PeekRight();

            Assert.Equal(new[] {1, 2, 3}, deque);
        }
    }
}
