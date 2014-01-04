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

            //clean local strong references
            obj1 = obj2 = null;

            deque.Clear();
            
            //assert that all strong references to the two objects have been cleaned
            GC.Collect();
            Assert.False(ref1.IsAlive);
            Assert.False(ref2.IsAlive);

            /*
             * Make sure the GC doesn't clean the deque and all its references before this.
             * If it did, the above assertions could be "false positives" - the references could have been cleaned
             * not because Deque<T>.Clear worked, but because the local "deque" variable is not used after the call.
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

            //clean local strong references
            obj1 = obj2 = obj3 = obj4 = null;

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
    }
}
