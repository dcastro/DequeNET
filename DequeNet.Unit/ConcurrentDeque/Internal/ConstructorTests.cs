using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using DequeNet.Test.Common;

namespace DequeNet.Unit.ConcurrentDeque.Internal
{
    public class ConstructorTests
    {
        [Fact]
        public void PointersAreNull_ByDefault()
        {
            //Act
            var deque = new ConcurrentDeque<int>();

            //Assert
            var anchor = deque._anchor;
            Assert.Null(anchor._left);
            Assert.Null(anchor._right);
        }

        [Fact]
        public void IsStable_ByDefault()
        {
            //Act
            var deque = new ConcurrentDeque<int>();

            //Assert
            var anchor = deque._anchor;
            Assert.Equal(ConcurrentDeque<int>.DequeStatus.Stable, anchor._status);
        }

        [Theory]
        [PropertyData("Items")]
        public void WithEnumerable_MaintainsPointersIntegrity(int[] collection)
        {
            var deque = new ConcurrentDeque<int>(collection);

            Assert.Equal(collection, deque.GetNodes().Select(n => n._value));
            Assert.Equal(collection.Reverse(), deque.GetNodesReverse().Select(n => n._value));
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
