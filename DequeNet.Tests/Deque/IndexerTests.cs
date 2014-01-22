using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Tests.Deque
{
    public class IndexerTests
    {
        [Fact]
        public void NegativeIndex_ThrowsException()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            Assert.Throws<ArgumentOutOfRangeException>(() => deque[-1]);
        }

        [Fact]
        public void IndexGreaterThanCount_ThrowsException()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            Assert.Throws<ArgumentOutOfRangeException>(() => deque[3]);
        }

        [Fact]
        public void Indexer_Returns_ItemAtTheGivenIndex()
        {
            //make deque "wrap around" the ring buffer
            var deque = new Deque<int>(new[] {1, 2, 3, 4, 5});
            deque.PopLeft();
            deque.PopLeft();
            deque.PushRight(6);

            int[] expectedSequence = {3, 4, 5, 6};

            for (int i = 0; i < deque.Count; i++)
                Assert.Equal(expectedSequence[i], deque[i]);
        }

        [Fact]
        public void Setter_ChangesItem_AtTheGivenIndex()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});

            deque[1] = 4;

            Assert.Equal(new[] {1, 4, 3}, deque);
        }
    }
}
