using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit.ConcurrentDeque
{
    public class TryPopTests
    {
        [Fact]
        public void TryPopRight_FailsOnEmptyDeque()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();

            //Act & Assert
            int item;
            Assert.False(deque.TryPopRight(out item));
            Assert.Equal(item, default(int));
        }

        [Fact]
        public void TryPopRight_ReturnsTheLastRemainingItem()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);

            //Act & Assert
            int item;
            Assert.True(deque.TryPopRight(out item));
            Assert.Equal(item, 1);
            Assert.Equal(0, deque.Count);
        }

        [Fact]
        public void TryPopRight_ReturnsTheRightmostItem()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>(new[] {1, 3, 5});

            //Act & Assert
            int item;
            Assert.True(deque.TryPopRight(out item));
            Assert.Equal(item, 5);
            Assert.Equal(new[] {1, 3}, deque);
        }

        [Fact]
        public void TryPopLeft_FailsOnEmptyDeque()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();

            //Act & Assert
            int item;
            Assert.False(deque.TryPopLeft(out item));
            Assert.Equal(item, default(int));
        }

        [Fact]
        public void TryPopLeft_ReturnsTheLastRemainingItem()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);

            //Act & Assert
            int item;
            Assert.True(deque.TryPopLeft(out item));
            Assert.Equal(item, 1);
            Assert.Equal(0, deque.Count);
        }

        [Fact]
        public void TryPopLeft_ReturnsTheLeftmostItem()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>(new[] {1, 3, 5});

            //Act & Assert
            int item;
            Assert.True(deque.TryPopLeft(out item));
            Assert.Equal(item, 1);
            Assert.Equal(new[] {3, 5}, deque);
        }
    }
}
