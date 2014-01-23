using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using DequeNet.Tests.Helpers;

namespace DequeNet.Tests.Deque
{
    public class CollectionTests
    {
        [Fact]
        public void CopyTo_WithMultiDimensionalArray_ThrowsExeption()
        {
            ICollection deque = new Deque<int>(new[] {1, 2, 3});
            var array = new int[2,2];

            var ex = Assert.Throws<ArgumentException>(() => deque.CopyTo(array, 0));
            Assert.Contains("dimension", ex.Message, StringComparison.InvariantCultureIgnoreCase);

            array.AssertNotCorrupted<int>();
        }

        [Fact]
        public void CopyTo_WithArrayOfWrongType_ThrowsException()
        {
            ICollection deque = new Deque<int>(new[] {1, 2, 3});
            var array = new int[3][];

            var ex = Assert.Throws<ArgumentException>(() => deque.CopyTo(array, 0));
            Assert.Contains("array type", ex.Message, StringComparison.InvariantCultureIgnoreCase);

            array.AssertNotCorrupted();
        }

        [Fact]
        public void CopyTo_WithNullArray_ThrowsException()
        {
            ICollection deque = new Deque<int>(new[] {1, 2, 3});
            Assert.Throws<ArgumentNullException>(() => deque.CopyTo(null, 0));
        }

        [Fact]
        public void CopyTo_WithNegativeIndex_ThrowsException()
        {
            ICollection deque = new Deque<int>(new[] {1, 2, 3});
            var array = new int[1];

            Assert.Throws<ArgumentOutOfRangeException>(() => deque.CopyTo(array, -1));
            array.AssertNotCorrupted();
        }

        [Fact]
        public void CopyTo_WithIndexEqualToArrayLength_ThrowsException()
        {
            ICollection deque = new Deque<int>(new[] {1, 2, 3});
            var array = new int[1];

            Assert.Throws<ArgumentOutOfRangeException>(() => deque.CopyTo(array, 1));
            array.AssertNotCorrupted();
        }

        [Fact]
        public void CopyTo_WithIndexGreaterThanArrayLength_ThrowsException()
        {
            ICollection deque = new Deque<int>(new[] {1, 2, 3});
            var array = new int[1];

            Assert.Throws<ArgumentOutOfRangeException>(() => deque.CopyTo(array, 2));
            array.AssertNotCorrupted();
        }

        [Fact]
        public void CopyTo_ThrowsException_IfArrayIsntLongEnough()
        {
            ICollection deque = new Deque<int>(new[] {1, 2, 3});
            var array1 = new int[2];
            var array2 = new int[3];

            Assert.Throws<ArgumentException>(() => deque.CopyTo(array1, 0));
            Assert.Throws<ArgumentException>(() => deque.CopyTo(array2, 1));

            array1.AssertNotCorrupted();
            array2.AssertNotCorrupted();
        }

        [Fact]
        public void CopyTo_CopiesDequesContent()
        {
            ICollection deque = new Deque<string>(new[] {"1", "2", "3"});
            var array = new string[5];
            deque.CopyTo(array, 1);

            Assert.Null(array[0]);
            Assert.Null(array[4]);
            Assert.Equal(array.Skip(1).Take(3), deque.Cast<string>());
        }

        [Fact]
        public void CopyTo_CopiesDequesContent_WhenDequeLoopsAround()
        {
            var deque = new Deque<string>(new[] {"1", "2", "3"});
            deque.PopRight();
            deque.PushLeft("0");

            var array = new string[5];
            ((ICollection) deque).CopyTo(array, 1);

            Assert.Null(array[0]);
            Assert.Null(array[4]);
            Assert.Equal(array.Skip(1).Take(3), deque);
        }

        [Fact]
        public void IsSynchronized_ReturnsFalse()
        {
            ICollection deque = new Deque<int>();
            Assert.False(deque.IsSynchronized);
        }

        [Fact]
        public void SyncRoot_ReturnsObject()
        {
            ICollection deque = new Deque<int>();
            Assert.NotNull(deque.SyncRoot);
        }
    }
}
