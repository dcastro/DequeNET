using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DequeNet
{
    public class ConcurrentDeque<T> : IConcurrentDeque<T>
    {
        //Disable "a reference to a volatile field will not be treated as volatile" warnings.
        //As per the MSDN documentation: "There are exceptions to this, such as when calling an interlocked API".
        #pragma warning disable 420

        // ReSharper disable InconsistentNaming
        internal volatile Anchor _anchor;

        public bool IsEmpty
        {
            get { return _anchor._left == null; }
        }

        public ConcurrentDeque()
        {
            _anchor = new Anchor();
        }
 
        public ConcurrentDeque(IEnumerable<T> items)
        {
            throw new NotImplementedException();
        }  
        
        public void PushRight(T item)
        {
            var newNode = new Node(item);

            while (true)
            {
                var anchor = _anchor;

                //If the deque is empty
                if (anchor._right == null)
                {
                    //update both pointers to point to the new node
                    var newAnchor = new Anchor(newNode, newNode, anchor._status);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        return;
                }
                else if (anchor._status == DequeStatus.Stable)
                {
                    //make new node point to the rightmost node
                    newNode._left = anchor._right;

                    //update the anchor's right pointer
                    //and change the status to RPush
                    var newAnchor = new Anchor(anchor._left, newNode, DequeStatus.RPush);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                    {
                        //stabilize deque
                        StabilizeRight(newAnchor);
                        return;
                    }
                }
                else
                {
                    //if the deque is unstable,
                    //attempt to bring it to a stable state before trying to insert the node.
                    Stabilize(anchor);
                }
            }
        }

        public void PushLeft(T item)
        {
            var newNode = new Node(item);

            while (true)
            {
                var anchor = _anchor;

                //If the deque is empty
                if (anchor._left == null)
                {
                    //update both pointers to point to the new node
                    var newAnchor = new Anchor(newNode, newNode, anchor._status);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        return;
                }
                else if (anchor._status == DequeStatus.Stable)
                {
                    //make new node point to the leftmost node
                    newNode._right = anchor._left;

                    //update anchor's left pointer
                    //and change the status to LPush
                    var newAnchor = new Anchor(newNode, anchor._right, DequeStatus.LPush);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                    {
                        //stabilize deque
                        StabilizeLeft(newAnchor);
                        return;
                    }
                }
                else
                {
                    //if the deque is unstable,
                    //attempt to bring it to a stable state before trying to insert the node.
                    Stabilize(anchor);
                }
            }
        }

        public bool TryPopRight(out T item)
        {
            Anchor anchor;
            while (true)
            {
                anchor = _anchor;
                
                if (anchor._right == null)
                {
                    //return false if the deque is empty
                    item = default(T);
                    return false;
                }
                if (anchor._right == anchor._left)
                {
                    //update both pointers if the deque has only one node
                    var newAnchor = new Anchor(null, null, DequeStatus.Stable);
                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        break;
                }
                else if (anchor._status == DequeStatus.Stable)
                {
                    //update right pointer if deque has > 1 node
                    var prev = anchor._right._left;
                    var newAnchor = new Anchor(anchor._left, prev, anchor._status);
                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        break;
                }
                else
                {
                    //if the deque is unstable,
                    //attempt to bring it to a stable state before trying to remove the node.
                    Stabilize(anchor);
                }
            }

            var node = anchor._right;
            item = node._value;

            /**
             * Try to set the new rightmost node's right pointer to null to avoid memory leaks.
             * We try only once - if CAS fails, then another thread must have pushed a new node, in which case we simply carry on.
             */
            var rightmostNode = node._left;
            if (rightmostNode != null)
                Interlocked.CompareExchange(ref rightmostNode._right, null, node);

            return true;
        }

        public bool TryPopLeft(out T item)
        {
            Anchor anchor;
            while (true)
            {
                anchor = _anchor;

                if (anchor._left == null)
                {
                    //return false if the deque is empty
                    item = default(T);
                    return false;
                }
                if (anchor._right == anchor._left)
                {
                    //update both pointers if the deque has only one node
                    var newAnchor = new Anchor(null, null, DequeStatus.Stable);
                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        break;
                }
                else if (anchor._status == DequeStatus.Stable)
                {
                    //update left pointer if deque has > 1 node
                    var prev = anchor._left._right;
                    var newAnchor = new Anchor(prev, anchor._right, anchor._status);
                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        break;
                }
                else
                {
                    //if the deque is unstable,
                    //attempt to bring it to a stable state before trying to remove the node.
                    Stabilize(anchor);
                }
            }

            var node = anchor._left;
            item = node._value;
            
            /**
             * Try to set the new leftmost node's left pointer to null to avoid memory leaks.
             * We try only once - if CAS fails, then another thread must have pushed a new node, in which case we simply carry on.
             */
            var leftmostNode = node._right;
            if (leftmostNode != null)
                Interlocked.CompareExchange(ref leftmostNode._left, null, node);

            return true;
        }

        private void Stabilize(Anchor anchor)
        {
            if(anchor._status == DequeStatus.RPush)
                StabilizeRight(anchor);
            else
                StabilizeLeft(anchor);
        }

        /// <summary>
        /// Stabilizes the deque when in <see cref="DequeStatus.RPush"/> status.
        /// </summary>
        /// <remarks>
        /// Stabilization is done in two steps:
        /// (1) update the previous rightmost node's right pointer to point to the new rightmost node;
        /// (2) update the anchor, changing the status to <see cref="DequeStatus.Stable"/>.
        /// </remarks>
        /// <param name="anchor"></param>
        private void StabilizeRight(Anchor anchor)
        {
            //quick check to see if the anchor has been updated by another thread
            if (_anchor != anchor)
                return;

            //grab a reference to the new node
            var newNode = anchor._right;

            //grab a reference to the previous rightmost node and its right pointer
            var prev = newNode._left;
            var prevNext = prev._right;

            //if the previous rightmost node doesn't point to the new rightmost node, we need to update it
            if (prevNext != newNode)
            {
                /**
                 * Quick check to see if the anchor has been updated by another thread.
                 * If it has been updated, we can't touch the prev node.
                 * Some other thread may have popped the new node, pushed another node and stabilized the deque.
                 * If that's the case, then prev node's right pointer is pointing to the other node.
                 */
                if (_anchor != anchor)
                    return;

                //try to make the previous rightmost node point to the next node.
                //CAS failure means that another thread already stabilized the deque.
                if (Interlocked.CompareExchange(ref prev._right, newNode, prevNext) != prevNext)
                    return;
            }

            /**
             * Try to mark the anchor as stable.
             * This step is done outside of the previous "if" block:
             *   even though another thread may have already updated prev's right pointer,
             *   this thread might still preempt the other and perform the second step (i.e., update the anchor).
             */
            var newAnchor = new Anchor(anchor._left, anchor._right, DequeStatus.Stable);
            Interlocked.CompareExchange(ref _anchor, newAnchor, anchor);
        }

        /// <summary>
        /// Stabilizes the deque when in <see cref="DequeStatus.LPush"/> status.
        /// </summary>
        /// <remarks>
        /// Stabilization is done in two steps:
        /// (1) update the previous leftmost node's left pointer to point to the new leftmost node;
        /// (2) update the anchor, changing the status to <see cref="DequeStatus.Stable"/>.
        /// </remarks>
        /// <param name="anchor"></param>
        private void StabilizeLeft(Anchor anchor)
        {
            //quick check to see if the anchor has been updated by another thread
            if (_anchor != anchor)
                return;

            //grab a reference to the new node
            var newNode = anchor._left;

            //grab a reference to the previous leftmost node and its left pointer
            var prev = newNode._right;
            var prevNext = prev._left;

            //if the previous leftmost node doesn't point to the new leftmost node, we need to update it
            if (prevNext != newNode)
            {
                /**
                 * Quick check to see if the anchor has been updated by another thread.
                 * If it has been updated, we can't touch the prev node.
                 * Some other thread may have popped the new node, pushed another node and stabilized the deque.
                 * If that's the case, then prev node's left pointer is pointing to the other node.
                 */
                if (_anchor != anchor)
                    return;

                //try to make the previous leftmost node point to the next node.
                //CAS failure means that another thread already stabilized the deque.
                if (Interlocked.CompareExchange(ref prev._left, newNode, prevNext) != prevNext)
                    return;
            }

            /**
             * Try to mark the anchor as stable.
             * This step is done outside of the previous "if" block:
             *   even though another thread may have already updated prev's left pointer,
             *   this thread might still preempt the other and perform the second step (i.e., update the anchor).
             */
            var newAnchor = new Anchor(anchor._left, anchor._right, DequeStatus.Stable);
            Interlocked.CompareExchange(ref _anchor, newAnchor, anchor);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ConcurrentDeque{T}"/> from left to right.
        /// </summary>
        /// <returns>An enumerator for the <see cref="ConcurrentDeque{T}"/>.</returns>
        /// <remarks>
        /// The enumerator iterates through the nodes in the deque from left to right and is safe to use concurrently with
        /// reads and writes to the deque.
        /// However, it does not represent a moment-in-time snapshot of the deque.
        /// Items popped from the left end and items pushed onto the right end after <see cref="GetEnumerator"/> was called may be exposed through the enumerator.
        /// Items popped from the right end and items pushed onto the the left end after <see cref="GetEnumerator"/> was called will not be exposed.
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            //fetch the leftmost node
            var current = _anchor._left;

            //Loop until we go out of bounds
            while (current != null)
            {
                yield return current._value;
                current = current._right;
            }
        }

        /// <summary>
        /// Returns an enumerable sequence of the elements in the <see cref="ConcurrentDeque{T}"/> in reverse order.
        /// </summary>
        /// <returns>A reverse sequence of the elements in the <see cref="ConcurrentDeque{T}"/>.</returns>
        /// <remarks>
        /// The enumerable's enumerator iterates through the nodes in the deque from right to left and is safe to use concurrently with
        /// reads and writes to the deque.
        /// However, it does not represent a moment-in-time snapshot of the deque.
        /// Items popped from the right end and items pushed onto the left end after <see cref="IEnumerable{T}.GetEnumerator"/> was called may be exposed through the enumerator.
        /// Items popped from the left end and items pushed onto the the right end after <see cref="IEnumerable{T}.GetEnumerator"/> was called will not be exposed.
        /// </remarks>
        public IEnumerable<T> Reverse()
        {
            //fetch the rightmost node
            var current = _anchor._right;

            //Loop until we go out of bounds
            while (current != null)
            {
                yield return current._value;
                current = current._left;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get; private set; }
        public object SyncRoot { get; private set; }
        public bool IsSynchronized { get; private set; }
        
        public bool TryAdd(T item)
        {
            throw new NotImplementedException();
        }

        public bool TryTake(out T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection"/> to an <see cref="T:System.Array"/>,
        /// starting at a particular <see cref="T:System.Array"/> index. 
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the destination of the elements copied from the <see cref="ICollection"/>. The <see cref="T:System.Array">Array</see> must have zero-based indexing.</param> 
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is a null reference (Nothing in Visual Basic).</exception> 
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.</exception> 
        /// <exception cref="ArgumentException"> 
        /// <paramref name="array"/> is multidimensional. -or-
        /// <paramref name="array"/> does not have zero-based indexing. -or- 
        /// <paramref name="index"/> is equal to or greater than the length of the <paramref name="array"/>
        /// -or- The number of elements in the source <see cref="ICollection"/> is
        /// greater than the available space from <paramref name="index"/> to the end of the destination
        /// <paramref name="array"/>. -or- The type of the source <see cref="ICollection"/> cannot be cast
        /// automatically to the type of the destination <paramref name="array"/>. 
        /// </exception> 
        void ICollection.CopyTo(Array array, int index)
        {
            // We must be careful not to corrupt the array, so we will first accumulate an 
            // internal list of elements that we will then copy to the array. This requires
            // some extra allocation, but is necessary since we don't know up front whether 
            // the array is sufficiently large to hold the stack's contents.
            ((ICollection) ToList()).CopyTo(array, index);
        }

        /// <summary>
        /// Copies the <see cref="ConcurrentDeque{T}"/> elements to an existing one-dimensional <see cref="T:System.Array">Array</see>, starting at the specified array index. 
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="T:System.Array">Array</see> that is the destination of the elements copied from the <see cref="ConcurrentDeque{T}"/>. The <see cref="T:System.Array">Array</see> must have zero-based indexing.
        /// </param> 
        /// <param name="index">
        /// The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference (Nothing in Visual Basic).
        /// </exception> 
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero.
        /// </exception> 
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> is equal to or greater than the length of the <paramref name="array"/>
        /// -or- The number of elements in the source <see cref="ConcurrentDeque{T}"/> is greater than the 
        /// available space from <paramref name="index"/> to the end of the destination <paramref
        /// name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            // We must be careful not to corrupt the array, so we will first accumulate an
            // internal list of elements that we will then copy to the array. This requires
            // some extra allocation, but is necessary since we don't know up front whether 
            // the array is sufficiently large to hold the stack's contents.
            ToList().CopyTo(array, index);
        }

        /// <summary> 
        /// Copies the items stored in the <see cref="ConcurrentDeque{T}"/> to a new array.
        /// </summary> 
        /// <returns>
        /// A new array containing a snapshot of elements copied from the <see cref="ConcurrentDeque{T}"/>.
        /// </returns>
        public T[] ToArray()
        {
            return ToList().ToArray();
        }

        private List<T> ToList()
        {
            return new List<T>(this);
        } 

        internal class Anchor
        {
            internal readonly Node _left;
            internal readonly Node _right;
            internal readonly DequeStatus _status;

            internal Anchor()
            {
                _right = _left = null;
                _status = DequeStatus.Stable;
            }

            internal Anchor(Node left, Node right, DequeStatus status)
            {
                _left = left;
                _right = right;
                _status = status;
            }
        }

        internal enum DequeStatus
        {
            Stable,
            LPush,
            RPush
        };

        internal class Node
        {
            internal volatile Node _left;
            internal volatile Node _right;
            internal readonly T _value;

            internal Node(T value)
            {
                _value = value;
            }
        }

        #pragma warning restore 420
        // ReSharper restore InconsistentNaming
    }
}
