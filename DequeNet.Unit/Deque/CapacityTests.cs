using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit.Deque
{
    public class CapacityTests
    {
        [Fact]
        public void Capacity_IsZeroByDefault()
        {
            var deque = new Deque<int>();
            Assert.Equal(0, deque.Capacity);
        }

        [Fact]
        public void Pushing_IncreasesCapacity()
        {
            var deque = new Deque<int>();
            deque.PushRight(5);

            Assert.Equal(4, deque.Capacity);
        }

        [Fact]
        public void Pushing_DoublesCapacity()
        {
            var deque = new Deque<int>();

            for (int i = 0; i < 5; i++)
                deque.PushRight(5);

            Assert.Equal(8, deque.Capacity);
        }
    }
}
