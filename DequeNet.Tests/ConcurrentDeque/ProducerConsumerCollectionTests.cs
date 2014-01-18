using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace DequeNet.Unit.ConcurrentDeque
{
    public class ProducerConsumerCollectionTests
    {
        [Fact]
        public void TryAdd_AppendsValue_ToTheRight()
        {
            //Act
            IProducerConsumerCollection<int> deque = new ConcurrentDeque<int>();
            deque.TryAdd(2);
            deque.TryAdd(3);

            //Assert
            Assert.Equal(new[] {2, 3}, deque);
        }

        [Fact]
        public void TryTake_TakesValue_FromTheLeft()
        {
            //Arrange
            IProducerConsumerCollection<int> deque = new ConcurrentDeque<int>(new[] {1, 2});

            //Act
            int item;
            deque.TryTake(out item);

            //Assert
            Assert.Equal(1, item);
            Assert.Equal(new[] {2}, deque);
        }

        [Fact]
        public void ToArray_ReturnsSnapshot()
        {
            //Arrange
            int[] array = {0, 1, 2};
            var deque = new ConcurrentDeque<int>(array);

            //Act
            var snapshot = deque.ToArray();

            //Assert
            Assert.Equal(array, snapshot);
        }

        [Fact]
        public void CopyTo_CopiesItems()
        {
            //Arrange
            int[] array = {0, 1, 2};
            var deque = new ConcurrentDeque<int>(array);

            //Act
            var copy = new int[3];
            deque.CopyTo(copy, 0);

            //Assert
            Assert.Equal(array, copy);
        }

        [Fact]
        public void CollectionCopyTo_CopiesItems()
        {
            //Arrange
            int[] array = {0, 1, 2};
            var deque = new ConcurrentDeque<int>(array);

            //Act
            var copy = new int[3];
            ((ICollection) deque).CopyTo(copy, 0);

            //Assert
            Assert.Equal(array, copy);
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
        [PropertyData("Items")]
        public void CountReturnsTheNumberOfItemsInTheDeque(int[] items)
        {
            var deque = new ConcurrentDeque<int>(items);

            Assert.Equal(items.Length, deque.Count);
        }

        [Fact]
        public void EnumeratorDoesNotIncludeConcurrentModifications()
        {
            //Arrange
            var arr = new[] { 1, 2, 3 };
            var deque = new ConcurrentDeque<int>(arr);
            int item;

            //Act
            var iterator = deque.GetEnumerator();
            iterator.MoveNext();

            deque.TryPopLeft(out item);
            deque.TryPopLeft(out item);
            deque.PushLeft(6);

            deque.TryPopRight(out item);
            deque.PushRight(6);

            //Assert
            Assert.Equal(1, iterator.Current);

            Assert.True(iterator.MoveNext());
            Assert.Equal(2, iterator.Current);

            Assert.True(iterator.MoveNext());
            Assert.Equal(3, iterator.Current);

            Assert.False(iterator.MoveNext());
        }

        public static IEnumerable<object[]> Items
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
