using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace DequeNet.Unit.ConcurrentDeque
{
    public class ConstructorTests
    {
        [Fact]
        public void HasNoItems_ByDefault()
        {
            var deque = new ConcurrentDeque<int>();
            Assert.Empty(deque);
        }

        [Theory]
        [PropertyData("Items")]
        public void WithIEnumerable_CopiesCollection(int[] collection)
        {
            int[] array = {1, 2, 3, 4};
            var deque = new ConcurrentDeque<int>(array);

            Assert.Equal(array, deque);
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
