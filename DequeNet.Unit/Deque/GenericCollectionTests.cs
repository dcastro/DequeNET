using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit.Deque
{
    public class GenericCollectionTests
    {
        [Fact]
        public void Add_AppendsItemToTheRightEnd()
        {
            ICollection<int> deque = new Deque<int>(new[] {1, 2, 3});
            deque.Add(4);

            Assert.Equal(4, deque.Count);
            Assert.Equal(new[] {1, 2, 3, 4}, deque as Deque<int>);
        }

        [Fact]
        public void Contains_ReturnsTrue_IfDequeHasItem()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});

            Assert.True(deque.Contains(2));
        }

        [Fact]
        public void Contains_ReturnsFalse_IfDequeDoesntHaveItem()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });

            Assert.False(deque.Contains(4));
        }
    }
}
