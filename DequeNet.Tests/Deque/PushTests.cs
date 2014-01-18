using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit.Deque
{
    public class PushTests
    {
        [Fact]
        public void PushRight_AppendsItemToTheRightEnd()
        {
            var deque = new Deque<int>();
            deque.PushRight(1);
            deque.PushRight(2);
            deque.PushRight(3);

            Assert.Equal(new[] {1, 2, 3}, deque);
        }

        [Fact]
        public void PushRight_ToEmptyDeque_IncreasesCapacity()
        {
            var deque = new Deque<int>();
            deque.PushRight(5);

            Assert.Equal(4, deque.Capacity);
        }

        [Fact]
        public void PushRight_DoublesCapacity()
        {
            var deque = new Deque<int>();

            for (int i = 0; i < 5; i++)
                deque.PushRight(5);

            Assert.Equal(8, deque.Capacity);
        }

        [Fact]
        public void PushRight_IncreasesCount()
        {
            var deque = new Deque<int>();

            for (int i = 0; i < 5; i++)
                deque.PushRight(5);

            Assert.Equal(5, deque.Count);
        }

        [Fact]
        public void PushRight_LoopsAroundBuffer()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            deque.PopLeft();
            deque.PushRight(4);

            Assert.Equal(3, deque.Capacity);
            Assert.Equal(new[] {2, 3, 4}, deque);
        }

        [Fact]
        public void PushLeft_AppendsItemToTheLeftEnd()
        {
            var deque = new Deque<int>();
            deque.PushLeft(1);
            deque.PushLeft(2);
            deque.PushLeft(3);

            Assert.Equal(new[] {3, 2, 1}, deque);
        }

        [Fact]
        public void PushLeft_ToEmptyDeque_IncreasesCapacity()
        {
            var deque = new Deque<int>();
            deque.PushLeft(5);

            Assert.Equal(4, deque.Capacity);
        }

        [Fact]
        public void PushLeft_DoublesCapacity()
        {
            var deque = new Deque<int>();

            for (int i = 0; i < 5; i++)
                deque.PushLeft(5);

            Assert.Equal(8, deque.Capacity);
        }

        [Fact]
        public void PushLeft_IncreasesCount()
        {
            var deque = new Deque<int>();

            for (int i = 0; i < 5; i++)
                deque.PushLeft(5);

            Assert.Equal(5, deque.Count);
        }

        [Fact]
        public void PushLeft_LoopsAroundBuffer()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            deque.PopRight();
            deque.PushLeft(0);

            Assert.Equal(3, deque.Capacity);
            Assert.Equal(new[] {0, 1, 2}, deque);
        }
    }
}
