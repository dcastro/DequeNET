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


        [Fact]
        public void Clear_ResetsDeque()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            deque.Clear();

            Assert.Equal(0, deque.Count);
            Assert.Equal(new int[0], deque);
        }

        [Fact]
        public void Clear_KeepsCapacity()
        {
            var deque = new Deque<int>(6);
            deque.Clear();

            Assert.Equal(6, deque.Capacity);
        }

#if !DEBUG 
        [Fact]
        public void Clear_PurgesReferences()
        {
            var obj1 = new object();
            var obj2 = new object();

            var ref1 = new WeakReference(obj1);
            var ref2 = new WeakReference(obj2);

            var deque = new Deque<object>(new[] {obj1, obj2});

            deque.Clear();
            
            //assert that all strong references to the two objects have been cleaned
            GC.Collect();
            Assert.False(ref1.IsAlive);
            Assert.False(ref2.IsAlive);

            /*
             * Make sure the GC doesn't clean the deque and all its references before this.
             * If it did, the above assertions could be "false positives" - the references could have been collected
             * not because Deque<T>.Clear worked, but because the deque itself was collected.
             */
            GC.KeepAlive(deque);
        }
#endif

#if !DEBUG 
        [Fact]
        public void Clear_PurgesReferences_WhenDequeLoopsAround()
        {
            var obj1 = new object();
            var obj2 = new object();
            var obj3 = new object();
            var obj4 = new object();

            var ref1 = new WeakReference(obj1);
            var ref2 = new WeakReference(obj2);
            var ref3 = new WeakReference(obj3);
            var ref4 = new WeakReference(obj4);

            var deque = new Deque<object>(new[] {obj1, obj2, obj3});
            deque.PopLeft();
            deque.PushRight(obj4);

            deque.Clear();

            //assert that all strong references to the two objects have been cleaned
            GC.Collect();
            Assert.False(ref1.IsAlive);
            Assert.False(ref2.IsAlive);
            Assert.False(ref3.IsAlive);
            Assert.False(ref4.IsAlive);

            //Make sure the GC doesn't clean the deque and all its references before this.
            GC.KeepAlive(deque);
        }
#endif

        [Fact]
        public void CopyTo_WithNullArray_ThrowsException()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            Assert.Throws<ArgumentNullException>(() => deque.CopyTo(null, 0));
        }

        [Fact]
        public void CopyTo_WithNegativeIndex_ThrowsException()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>(() => deque.CopyTo(new int[1], -1));
        }

        [Fact]
        public void CopyTo_WithIndexEqualToArrayLength_ThrowsException()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>(() => deque.CopyTo(new int[1], 1));
        }

        [Fact]
        public void CopyTo_WithIndexGreaterThanArrayLength_ThrowsException()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentOutOfRangeException>(() => deque.CopyTo(new int[1], 2));
        }

        [Fact]
        public void CopyTo_ThrowsException_IfArrayIsntLongEnough()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            Assert.Throws<ArgumentException>(() => deque.CopyTo(new int[2], 0));
            Assert.Throws<ArgumentException>(() => deque.CopyTo(new int[3], 1));
        }

        [Fact]
        public void CopyTo_CopiesDequesContent()
        {
            var deque = new Deque<string>(new[] { "1", "2", "3" });
            var array = new string[5];
            deque.CopyTo(array, 1);

            Assert.Null(array[0]);
            Assert.Null(array[4]);
            Assert.Equal(array.Skip(1).Take(3), deque);
        }   
        
        [Fact]
        public void CopyTo_CopiesDequesContent_WhenDequeLoopsAround()
        {
            var deque = new Deque<string>(new[] { "1", "2", "3" });
            deque.PopRight();
            deque.PushLeft("0");

            var array = new string[5];
            deque.CopyTo(array, 1);

            Assert.Null(array[0]);
            Assert.Null(array[4]);
            Assert.Equal(array.Skip(1).Take(3), deque);
        }

        [Fact]
        public void Remove_UnknownItem_ThrowsException()
        {
            ICollection<int> deque = new Deque<int>(new[] {2, 3, 4});
            Assert.False(deque.Remove(5));
        }

        [Fact]
        public void Remove_LeftmostItem_RemovesItem()
        {
            ICollection<int> deque = new Deque<int>(new[] {2, 3, 4});

            Assert.True(deque.Remove(2));
            Assert.Equal(new[] {3, 4}, deque as IEnumerable<int>);
        }

        [Fact]
        public void Remove_RightmostItem_RemovesItem()
        {
            ICollection<int> deque = new Deque<int>(new[] {2, 3, 4});

            Assert.True(deque.Remove(4));
            Assert.Equal(new[] {2, 3}, deque as IEnumerable<int>);
        }

        [Fact]
        public void Remove_ItemTowardsTheLeftEnd_RemovesItem()
        {
            ICollection<int> deque = new Deque<int>(new[] {2, 3, 4, 5, 6, 7});

            Assert.True(deque.Remove(4));
            Assert.Equal(new[] {2, 3, 5, 6, 7}, deque as IEnumerable<int>);
        }

        [Fact]
        public void Remove_ItemTowardsTheLeftEnd_RemovesItem_WhenDequeLoopsAround()
        {
            var deque = new Deque<int>(new[] {2, 3, 4, 5, 6, 7});
            deque.PopRight();
            deque.PushLeft(1);

            Assert.True((deque as ICollection<int>).Remove(3));
            Assert.Equal(new[] {1, 2, 4, 5, 6}, deque);
        }

        [Fact]
        public void Remove_ItemTowardsTheRightEnd_RemovesItem()
        {
            ICollection<int> deque = new Deque<int>(new[] {2, 3, 4, 5, 6, 7});

            Assert.True(deque.Remove(5));
            Assert.Equal(new[] {2, 3, 4, 6, 7}, deque as IEnumerable<int>);
        }

        [Fact]
        public void Remove_ItemTowardsTheRightEnd_RemovesItem_WhenDequeLoopsAround()
        {
            var deque = new Deque<int>(new[] {2, 3, 4, 5, 6, 7});
            deque.PopLeft();
            deque.PushRight(8);

            Assert.True((deque as ICollection<int>).Remove(6));
            Assert.Equal(new[] {3, 4, 5, 7, 8}, deque);
        }
    }
}
