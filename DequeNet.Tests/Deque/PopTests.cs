using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit.Deque
{
    public class PopTests
    {
        [Fact]
        public void PopLeft_ThrowsException_WhenEmpty()
        {
            var deque = new Deque<int>();
            Assert.Throws<InvalidOperationException>(() => deque.PopLeft());
        }

        [Fact]
        public void PopLeft_ReturnsLeftmostItem()
        {
            var deque = new Deque<int>(new[] {1, 2, 3});
            var item = deque.PopLeft();

            Assert.Equal(1, item);
        }

        [Fact]
        public void PopLeft_DecreasesCount()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.PopLeft();

            Assert.Equal(2, deque.Count);
        }

#if !DEBUG 
        [Fact]
        public void PopLeft_ClearsReference()
        {
            var obj1 = new object();
            var ref1 = new WeakReference(obj1);

            var deque = new Deque<object>(new[] {obj1, new object(), new object()});
            deque.PopLeft();

            //assert that all strong references to the object have been cleaned
            GC.Collect();
            Assert.False(ref1.IsAlive);

            //Make sure the GC doesn't clean the deque and all its references before this.
            GC.KeepAlive(deque);
        }
#endif

        [Fact]
        public void PopRight_ThrowsException_WhenEmpty()
        {
            var deque = new Deque<int>();
            Assert.Throws<InvalidOperationException>(() => deque.PopRight());
        }

        [Fact]
        public void PopRight_ReturnsRightmostItem()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            var item = deque.PopRight();

            Assert.Equal(3, item);
        }

        [Fact]
        public void PopRight_DecreasesCount()
        {
            var deque = new Deque<int>(new[] { 1, 2, 3 });
            deque.PopRight();

            Assert.Equal(2, deque.Count);
        }

#if !DEBUG 
        [Fact]
        public void PopRight_ClearsReference()
        {
            var obj1 = new object();
            var ref1 = new WeakReference(obj1);

            var deque = new Deque<object>(new[] {new object(), new object(), obj1});
            deque.PopRight();

            //assert that all strong references to the object have been cleaned
            GC.Collect();
            Assert.False(ref1.IsAlive);

            //Make sure the GC doesn't clean the deque and all its references before this.
            GC.KeepAlive(deque);
        }
#endif
    }
}
