using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit
{
    public partial class ConcurrentDequeFixture
    {
        [Fact]
        public void DequeHasNoItemsByDefault()
        {
            //Act
            var deque = new ConcurrentDeque<int>();

            //Assert
            var anchor = deque._anchor;
            Assert.Null(anchor.Left);
            Assert.Null(anchor.Right);
        }

        [Fact]
        public void DequeIsStableByDefault()
        {
            //Act
            var deque = new ConcurrentDeque<int>();

            //Assert
            var anchor = deque._anchor;
            Assert.Equal(ConcurrentDeque<int>.DequeStatus.Stable, anchor.Status);
        }

        [Fact]
        public void PushRightAppendsNodeToEmptyList()
        {
            //Arrange
            const int value = 5;
            var deque = new ConcurrentDeque<int>();

            //Act
            deque.PushRight(value);

            //Assert
            var anchor = deque._anchor;
            Assert.NotNull(anchor.Right);
            Assert.NotNull(anchor.Left);
            Assert.Same(anchor.Left, anchor.Right);
            Assert.Equal(value, anchor.Right.Value);
        }

        [Fact]
        public void PushRightAppendsNodeToNonEmptyList()
        {
            //Arrange
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);

            //Act
            deque.PushRight(value);

            //Assert
            var anchor = deque._anchor;
            var newNode = anchor.Right;
            Assert.NotNull(newNode);
            Assert.Equal(value, newNode.Value);
        }

        [Fact]
        public void PushRightKeepsReferenceToPreviousRightNode()
        {
            //Arrange
            const int prevValue = 1;
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(prevValue);

            //Act
            deque.PushRight(value);

            //Assert
            var anchor = deque._anchor;
            var newNode = anchor.Right;
            Assert.Equal(prevValue, newNode.Left.Value);
        }

        [Fact]
        public void PushRightStabilizesDeque()
        {
            //Arrange
            const int prevValue = 1;
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(prevValue);

            //Act
            deque.PushRight(value);

            //Assert
            var anchor = deque._anchor;
            var newNode = anchor.Right;
            Assert.Same(newNode, newNode.Left.Right);
            Assert.Equal(ConcurrentDeque<int>.DequeStatus.Stable, anchor.Status);
        }

        [Fact]
        public void TryPopRightFailsOnEmptyDeque()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            
            //Act & Assert
            int item;
            Assert.False(deque.TryPopRight(out item));
            Assert.Equal(item, default(int));
        }

        [Fact]
        public void TryPopReturnsTheLastRemainingItem()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);

            //Act & Assert
            int item;
            Assert.True(deque.TryPopRight(out item));
            Assert.Equal(item, 1);

            int nodesCount = 0;
            ForEachNode(deque, n => nodesCount++);
            Assert.Equal(0, nodesCount);
        }

        [Fact]
        public void TryPopReturnsTheRightmostItem()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);
            deque.PushRight(3);
            deque.PushRight(5);

            //Act & Assert
            int item;
            Assert.True(deque.TryPopRight(out item));
            Assert.Equal(item, 5);

            int nodesCount = 0;
            ForEachNode(deque, n => nodesCount++);
            Assert.Equal(2, nodesCount);
        }

        private void ForEachNode<T>(ConcurrentDeque<T> deque, Action<ConcurrentDeque<T>.Node> action)
        {
            var anchor = deque._anchor;
            var current = anchor.Left;
            var last = anchor.Right;

            if (current == null)
                return;

            while (current != last)
            {
                action(current);
                current = current.Right;
            }
            action(last);
        }

        private void ReverseForEachNode<T>(ConcurrentDeque<T> deque, Action<ConcurrentDeque<T>.Node> action)
        {
            var anchor = deque._anchor;
            var current = anchor.Right;
            var first = anchor.Left;

            if (current == null)
                return;

            while (current != first)
            {
                action(current);
                current = current.Left;
            }
            action(first);
        }
    }
}
