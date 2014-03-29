using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Extensions;

namespace DequeNet.Tests.Deque
{
    public class EnumeratorTests
    {
        [Theory]
        [PropertyData("Mutations")]
        public void Mutations_InvalidateEnumerator(Action<Deque<int>> mutate)
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            IEnumerator<int> enumerator = deque.GetEnumerator();

            mutate(deque);

            Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
            Assert.Throws<InvalidOperationException>(() => enumerator.Reset());
        }

        [Theory]
        [PropertyData("Items")]
        public void Returns_CorrectContent(int[] collection)
        {
            var deque = new Deque<int>(collection);
            Assert.Equal(collection, deque);
        }

        [Fact]
        public void Reset_RestartsEnumerator()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});

            IEnumerator<int> enumerator = deque.GetEnumerator();
            enumerator.MoveNext();
            enumerator.MoveNext();

            //reset enumerator and move to the first element
            enumerator.Reset();
            enumerator.MoveNext();

            Assert.Equal(1, enumerator.Current);
        }

        [Fact]
        public void ChangingRingBufferLayout_DoesntInvalidateEnumerator()
        {
            //make deque "wrap around" the ring buffer
            var deque = new Deque<int>(new[] {1, 2, 3, 4, 5});
            deque.PopLeft();
            deque.PopLeft();
            deque.PushRight(6);

            //get enumerator and move to the first element
            IEnumerator<int> enumerator = deque.GetEnumerator();
            enumerator.MoveNext();

            //set expectations
            IEnumerable<int> shadowSequence = new[] {3, 4, 5, 6};
            IEnumerator<int> shadowEnumerator = shadowSequence.GetEnumerator();
            shadowEnumerator.MoveNext();

            //change ring buffer layout
            deque.TrimExcess();

            //Assert
            while (shadowEnumerator.MoveNext())
            {
                Assert.True(enumerator.MoveNext());
                Assert.Equal(shadowEnumerator.Current, enumerator.Current);
            }

            Assert.False(enumerator.MoveNext());
        }

        public static IEnumerable<object[]> Mutations
        {
            get
            {
                return new[]
                    {
                        new object[] {(Action<Deque<int>>) (deque => deque.PushRight(4))},
                        new object[] {(Action<Deque<int>>) (deque => deque.PushLeft(4))},
                        new object[] {(Action<Deque<int>>) (deque => deque.PopRight())},
                        new object[] {(Action<Deque<int>>) (deque => deque.PopLeft())},
                        new object[] {(Action<Deque<int>>) (deque => deque.Clear())},
                        new object[] {(Action<Deque<int>>) (deque => ((ICollection<int>) deque).Add(4))},
                        new object[] {(Action<Deque<int>>) (deque => ((ICollection<int>) deque).Remove(2))}
                    };
            }
        }

        public static IEnumerable<object[]> Items
        {
            get
            {
                return new[]
                    {
                        new object[] {new int[] {}},
                        new object[] {new[] {1}},
                        new object[] {new[] {1, 2, 3}},
                        new object[] {new[] {1, 2, 3, 4}}
                    };
            }
        }
    }
}
