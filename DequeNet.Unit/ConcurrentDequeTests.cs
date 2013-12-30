using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Xunit;
using DequeNet.Test.Common;
using Xunit.Extensions;

namespace DequeNet.Unit
{
    public partial class ConcurrentDequeTests
    {
        [Fact]
        public void DequeHasNoItemsByDefault()
        {
            //Act
            var deque = new ConcurrentDeque<int>();

            //Assert
            var anchor = deque._anchor;
            Assert.Null(anchor._left);
            Assert.Null(anchor._right);
        }

        [Fact]
        public void DequeIsStableByDefault()
        {
            //Act
            var deque = new ConcurrentDeque<int>();

            //Assert
            var anchor = deque._anchor;
            Assert.Equal(ConcurrentDeque<int>.DequeStatus.Stable, anchor._status);
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
            Assert.NotNull(anchor._right);
            Assert.NotNull(anchor._left);
            Assert.Same(anchor._left, anchor._right);
            Assert.Equal(value, anchor._right._value);
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
            var rightmostNode = anchor._right;
            Assert.NotNull(rightmostNode);
            Assert.Equal(value, rightmostNode._value);
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
            var newNode = anchor._right;
            Assert.Equal(prevValue, newNode._left._value);
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
            var newNode = anchor._right;
            Assert.Same(newNode, newNode._left._right);
            Assert.Equal(ConcurrentDeque<int>.DequeStatus.Stable, anchor._status);
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
        public void TryPopRightReturnsTheLastRemainingItem()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);

            //Act & Assert
            int item;
            Assert.True(deque.TryPopRight(out item));
            Assert.Equal(item, 1);

            long nodesCount = deque.GetNodes().LongCount();
            Assert.Equal(0, nodesCount);
        }

        [Fact]
        public void TryPopRightReturnsTheRightmostItem()
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

            long nodesCount = deque.GetNodes().LongCount();
            Assert.Equal(2, nodesCount);
        }

        [Fact]
        public void PushLeftAppendsNodeToEmptyList()
        {
            //Arrange
            const int value = 5;
            var deque = new ConcurrentDeque<int>();

            //Act
            deque.PushLeft(value);

            //Assert
            var anchor = deque._anchor;
            Assert.NotNull(anchor._right);
            Assert.NotNull(anchor._left);
            Assert.Same(anchor._left, anchor._right);
            Assert.Equal(value, anchor._left._value);
        }

        [Fact]
        public void PushLeftAppendsNodeToNonEmptyList()
        {
            //Arrange
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);

            //Act
            deque.PushLeft(value);

            //Assert
            var anchor = deque._anchor;
            var leftmostNode = anchor._left;
            Assert.NotNull(leftmostNode);
            Assert.Equal(value, leftmostNode._value);
        }

        [Fact]
        public void PushLeftKeepsReferenceToPreviousLeftNode()
        {
            //Arrange
            const int prevValue = 1;
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushLeft(prevValue);

            //Act
            deque.PushLeft(value);

            //Assert
            var anchor = deque._anchor;
            var newNode = anchor._left;
            Assert.Equal(prevValue, newNode._right._value);
        }

        [Fact]
        public void PushLeftStabilizesDeque()
        {
            //Arrange
            const int prevValue = 1;
            const int value = 5;
            var deque = new ConcurrentDeque<int>();
            deque.PushLeft(prevValue);

            //Act
            deque.PushLeft(value);

            //Assert
            var anchor = deque._anchor;
            var newNode = anchor._left;
            Assert.Same(newNode, newNode._right._left);
            Assert.Equal(ConcurrentDeque<int>.DequeStatus.Stable, anchor._status);
        }

        [Fact]
        public void TryPopLeftFailsOnEmptyDeque()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();

            //Act & Assert
            int item;
            Assert.False(deque.TryPopLeft(out item));
            Assert.Equal(item, default(int));
        }

        [Fact]
        public void TryPopLeftReturnsTheLastRemainingItem()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);

            //Act & Assert
            int item;
            Assert.True(deque.TryPopLeft(out item));
            Assert.Equal(item, 1);

            long nodesCount = deque.GetNodes().LongCount();
            Assert.Equal(0, nodesCount);
        }

        [Fact]
        public void TryPopLeftReturnsTheLeftmostItem()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);
            deque.PushRight(3);
            deque.PushRight(5);

            //Act & Assert
            int item;
            Assert.True(deque.TryPopLeft(out item));
            Assert.Equal(item, 1);

            long nodesCount = deque.GetNodes().LongCount();
            Assert.Equal(2, nodesCount);
        }

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

        [Fact]
        public void EnumeratorReturnsPushedNodes()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);
            deque.PushRight(2);
            deque.PushRight(3);
            deque.PushLeft(0);

            //Act & Assert
            Assert.Equal(new[] {0, 1, 2, 3}, deque);
        }

        [Fact]
        public void EnumeratorDoesNotReturnRightPoppedNodes()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(0);
            deque.PushRight(1);
            deque.PushRight(2);

            int item;
            deque.TryPopRight(out item);

            //Act & Assert
            Assert.Equal(new[] {0, 1}, deque);
        }

        [Fact]
        public void EnumeratorDoesNotReturnLeftPoppedNodes()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(0);
            deque.PushRight(1);
            deque.PushRight(2);

            int item;
            deque.TryPopLeft(out item);

            //Act & Assert
            Assert.Equal(new[] {1, 2}, deque);
        }

        [Fact]
        public void ToArrayReturnsSnapshot()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(0);
            deque.PushRight(1);
            deque.PushRight(2);

            //Act
            var array = deque.ToArray();

            //Assert
            Assert.Equal(new[] {0, 1, 2}, array);
        }

        [Fact]
        public void CopyToCopiesItems()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(0);
            deque.PushRight(1);
            deque.PushRight(2);

            //Act
            var array = new int[3];
            deque.CopyTo(array, 0);

            //Assert
            Assert.Equal(new[] { 0, 1, 2 }, array);
        }

        [Fact]
        public void CollectionCopyToCopiesItems()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(0);
            deque.PushRight(1);
            deque.PushRight(2);

            //Act
            var array = new int[3];
            ((ICollection) deque).CopyTo(array, 0);

            //Assert
            Assert.Equal(new[] { 0, 1, 2 }, array);
        }

        [Theory]
        [PropertyData("PushItems")]
        public void CountReturnsTheNumberOfItemsInTheDeque(int[] itemsToPush)
        {
            var deque = new ConcurrentDeque<int>();
            foreach (var item in itemsToPush)
            {
                deque.PushRight(item);
            }

            Assert.Equal(itemsToPush.Length, deque.Count);
        }

        [Fact]
        public void TryAddAppendsValueToTheRight()
        {
            //Act
            IProducerConsumerCollection<int> deque = new ConcurrentDeque<int>();
            deque.TryAdd(2);
            deque.TryAdd(3);

            //Assert
            Assert.Equal(3, deque.Last());
        }

        [Fact]
        public void TryTakeTakesValueFromTheLeft()
        {
            //Arrange
            IProducerConsumerCollection<int> deque = new ConcurrentDeque<int>();
            deque.TryAdd(2);
            deque.TryAdd(3);

            //Act
            int item;
            deque.TryTake(out item);

            //Assert
            Assert.Equal(2, item);
        }

        [Fact]
        public void SyncRootIsNotSupported()
        {
            IProducerConsumerCollection<int> deque = new ConcurrentDeque<int>();
            Assert.Throws<NotSupportedException>(() => deque.SyncRoot);
        }

        [Fact]
        public void IsSynchronizedReturnsFalse()
        {
            IProducerConsumerCollection<int> deque = new ConcurrentDeque<int>();
            Assert.False(deque.IsSynchronized);
        }

        [Theory]
        [PropertyData("PushItems")]
        public void DequeCanBeCreatedFromCollection(int[] collection)
        {
            var deque = new ConcurrentDeque<int>(collection);

            Assert.Equal(collection, deque);
            Assert.Equal(collection.Reverse(), deque.GetNodesReverse().Select(n => n._value));
        }

        [Fact]
        public void DequeCanBeSerialized()
        {
            //Arrange
            var array = new[] {1,2,3};
            var deque = new ConcurrentDeque<int>(array);

            ConcurrentDeque<int> deserializedDeque = null;

            //Act & Assert
            using (var ms = new MemoryStream())
            {
                //serialize
                var formatter = new BinaryFormatter();
                Assert.DoesNotThrow(() => formatter.Serialize(ms, deque));

                //deserialize
                ms.Seek(0, SeekOrigin.Begin);
                Assert.DoesNotThrow(() => deserializedDeque = formatter.Deserialize(ms) as ConcurrentDeque<int>);
            }

            //assert equivalence
            Assert.NotNull(deserializedDeque);
            Assert.Equal(array, deserializedDeque);
            Assert.Equal(array.Reverse(), deserializedDeque.GetNodesReverse().Select(n => n._value));
        }

        [Fact]
        public void ClearRemovesAllItems()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushLeft(5);

            //Act
            deque.Clear();

            //Assert
            Assert.True(deque.IsEmpty);
        }

        [Fact]
        public void TryPeekRightInspectsTheRightmostItem()
        {
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);
            deque.PushRight(2);
            deque.PushRight(3);

            int item;
            Assert.True(deque.TryPeekRight(out item));
            Assert.Equal(3, item);
            Assert.True(deque.Contains(3));
        }

        [Fact]
        public void TryPeekRightFailsIfDequeIsEmpty()
        {
            var deque = new ConcurrentDeque<int>();

            int item;
            Assert.False(deque.TryPeekRight(out item));
        }

        [Fact]
        public void TryPeekLeftInspectsTheLeftmostItem()
        {
            var deque = new ConcurrentDeque<int>();
            deque.PushRight(1);
            deque.PushRight(2);
            deque.PushRight(3);

            int item;
            Assert.True(deque.TryPeekLeft(out item));
            Assert.Equal(1, item);
            Assert.True(deque.Contains(1));
        }

        [Fact]
        public void TryPeekLeftFailsIfDequeIsEmpty()
        {
            var deque = new ConcurrentDeque<int>();

            int item;
            Assert.False(deque.TryPeekLeft(out item));
        }

        public static IEnumerable<object[]> PushItems
        {
            get
            {
                return new[]
                    {
                        new object[] {new int[] {}},
                        new object[] {new[] {1}},
                        new object[] {new[] {1, 1}}
                    };
            }
        }
    }
}
