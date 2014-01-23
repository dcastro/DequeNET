using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using DequeNet.Debugging;


//Disable "a reference to a volatile field will not be treated as volatile" warnings.
//As per the MSDN documentation: "There are exceptions to this, such as when calling an interlocked API".
#pragma warning disable 420

// ReSharper disable InconsistentNaming

namespace DequeNet
{
    /// <summary>
    /// Represents a thread-safe lock-free concurrent double-ended queue, also known as deque (pronounced "deck").
    /// Items can be appended to/removed from both ends of the deque.
    /// </summary>
    /// <typeparam name="T">Specifies the type of the elements in the deque.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ConcurrentDequeDebugView<>))]
    public class ConcurrentDeque<T> : IConcurrentDeque<T>
    {
        internal volatile Anchor _anchor;

        /// <summary>
        /// Gets a value that indicates whether the <see cref="ConcurrentDeque{T}"/> is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return _anchor._left == null; }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ConcurrentDeque{T}"/>.
        /// </summary>
        public int Count
        {
            get { return ToList().Count; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDeque{T}"/> class.
        /// </summary>
        public ConcurrentDeque()
        {
            _anchor = new Anchor();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentDeque{T}"/> class
        /// that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see cref="ConcurrentDeque{T}"/>.</param>
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="collection"/> argument is null.c
        /// </exception>
        public ConcurrentDeque(IEnumerable<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            InitializeFromCollection(collection);
        }

        /// <summary>
        /// Initialize the contents of the deque from an existing collection.
        /// </summary>
        /// <param name="collection">A collection from which to copy elements.</param>
        private void InitializeFromCollection(IEnumerable<T> collection)
        {
            var iterator = collection.GetEnumerator();
            if (iterator.MoveNext())
            {
                Node first = new Node(iterator.Current);
                Node last = first;

                while (iterator.MoveNext())
                {
                    //create node and assign pointers
                    Node newLast = new Node(iterator.Current)
                        {
                            _left = last
                        };
                    last._right = newLast;

                    //replace last node
                    last = newLast;
                }

                //initialize anchor
                _anchor = new Anchor(first, last, DequeStatus.Stable);
            }
            else
            {
                //collection is empty
                _anchor = new Anchor();
            }

        }

        /// <summary>
        /// Adds an item to the right end of the <see cref="ConcurrentDeque{T}"/>.
        /// </summary>
        /// <param name="item">The item to be added to the <see cref="ConcurrentDeque{T}"/>.</param>
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

        /// <summary>
        /// Adds an item to the left end of the <see cref="ConcurrentDeque{T}"/>.
        /// </summary>
        /// <param name="item">The item to be added to the <see cref="ConcurrentDeque{T}"/>.</param>
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

        /// <summary>
        /// Attempts to remove and return an item from the right end of the <see cref="ConcurrentDeque{T}"/>.
        /// </summary>
        /// <param name="item">When this method returns, if the operation was successful, <paramref name="item"/> contains the 
        /// object removed. If no object was available to be removed, the value is unspecified.</param>
        /// <returns>true if an element was removed and returned succesfully; otherwise, false.</returns>
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

            /*
             * Try to set the new rightmost node's right pointer to null to avoid memory leaks.
             * We try only once - if CAS fails, then another thread must have pushed a new node, in which case we simply carry on.
             */
            var rightmostNode = node._left;
            if (rightmostNode != null)
                Interlocked.CompareExchange(ref rightmostNode._right, null, node);

            return true;
        }

        /// <summary>
        /// Attempts to remove and return an item from the left end of the <see cref="ConcurrentDeque{T}"/>.
        /// </summary>
        /// <param name="item">When this method returns, if the operation was successful, <paramref name="item"/> contains the 
        /// object removed. If no object was available to be removed, the value is unspecified.</param>
        /// <returns>true if an element was removed and returned succesfully; otherwise, false.</returns>
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
            
            /*
             * Try to set the new leftmost node's left pointer to null to avoid memory leaks.
             * We try only once - if CAS fails, then another thread must have pushed a new node, in which case we simply carry on.
             */
            var leftmostNode = node._right;
            if (leftmostNode != null)
                Interlocked.CompareExchange(ref leftmostNode._left, null, node);

            return true;
        }

        /// <summary>
        /// Attempts to return the rightmost item of the <see cref="ConcurrentDeque{T}"/> 
        /// without removing it.
        /// </summary>
        /// <param name="item">When this method returns, <paramref name="item"/> contains the rightmost item
        /// of the <see cref="ConcurrentDeque{T}"/> or an unspecified value if the operation failed.</param>
        /// <returns>true if an item was returned successfully; otherwise, false.</returns>
        public bool TryPeekRight(out T item)
        {
            var rightmostNode = _anchor._right;

            if (rightmostNode != null)
            {
                item = rightmostNode._value;
                return true;
            }
            item = default(T);
            return false;
        }

        /// <summary>
        /// Attempts to return the leftmost item of the <see cref="ConcurrentDeque{T}"/> 
        /// without removing it.
        /// </summary>
        /// <param name="item">When this method returns, <paramref name="item"/> contains the leftmost item
        /// of the <see cref="ConcurrentDeque{T}"/> or an unspecified value if the operation failed.</param>
        /// <returns>true if an item was returned successfully; otherwise, false.</returns>
        public bool TryPeekLeft(out T item)
        {
            var leftmostNode = _anchor._left;

            if (leftmostNode != null)
            {
                item = leftmostNode._value;
                return true;
            }
            item = default(T);
            return false;
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

            //quick check to see if the deque has been modified by another thread
            if (prev == null)
                return;

            var prevNext = prev._right;

            //if the previous rightmost node doesn't point to the new rightmost node, we need to update it
            if (prevNext != newNode)
            {
                /*
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

            /*
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

            //quick check to see if the deque has been modified by another thread
            if (prev == null)
                return;

            var prevNext = prev._left;

            //if the previous leftmost node doesn't point to the new leftmost node, we need to update it
            if (prevNext != newNode)
            {
                /*
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

            /*
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
        /// The enumeration represents a moment-in-time snapshot of the contents
        /// of the deque. It does not reflect any updates to the collection after 
        /// <see cref="GetEnumerator"/> was called. The enumerator is safe to use 
        /// concurrently with reads from and writes to the deque.
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            return ToList().GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <remarks>
        /// The enumeration represents a moment-in-time snapshot of the contents
        /// of the deque. It does not reflect any updates to the collection after 
        /// <see cref="GetEnumerator"/> was called. The enumerator is safe to use 
        /// concurrently with reads from and writes to the deque.
        /// </remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Removes all objects from the <see cref="ConcurrentDeque{T}"/>.
        /// </summary>
        public void Clear()
        {
            /*
             * Clear the list by setting the anchor to a new one
             * with null left and right pointers.
             */
            _anchor = new Anchor();
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="ICollection"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">The SyncRoot property is not supported.</exception>
        object ICollection.SyncRoot
        {
            get { throw new NotSupportedException("The SyncRoot property may not be used for the synchronization of concurrent collections."); }
        }


        /// <summary>
        /// Gets a value indicating whether access to the <see cref="ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// true if access to the <see cref="ICollection"/> is synchronized (thread safe); otherwise, false.
        /// For <see cref="ConcurrentDeque{T}"/>, this property always returns false.
        /// </returns>
        bool ICollection.IsSynchronized
        {
            // Gets a value indicating whether access to this collection is synchronized. Always returns
            // false. The reason is subtle. While access is in fact thread safe, it's not the case that 
            // locking on the SyncRoot would have prevented concurrent pushes and pops, as this property 
            // would typically indicate; that's because we internally use CAS operations vs. true locks.
            get { return false; }
        }

        /// <summary>
        /// Attempts to add an object to the <see cref="IProducerConsumerCollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="IProducerConsumerCollection{T}"/>.</param>
        /// <returns>
        /// true if the object was added successfully; otherwise, false.
        /// </returns>
        /// <remarks>
        /// For <see cref="ConcurrentDeque{T}"/>, this operation will always add the object to the right
        /// end of the <see cref="ConcurrentDeque{T}"/> and return true.
        /// </remarks>
        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            PushRight(item);
            return true;
        }

        /// <summary>
        /// Attempts to remove and return an object from the <see cref="IProducerConsumerCollection{T}"/>.
        /// </summary>
        /// <param name="item">
        /// When this method returns, if the object was removed and returned successfully, <paramref name="item"/> contains the removed object.
        /// If no object was available to be removed, the value is unspecified.
        /// </param>
        /// <returns>
        /// true if an object was removed and returned successfully; otherwise, false.
        /// </returns>
        /// <remarks>
        /// For <see cref="ConcurrentDeque{T}"/>, this operation will attempt to remove the object
        /// from the left end of the <see cref="ConcurrentDeque{T}"/>. 
        /// </remarks>
        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            return TryPopLeft(out item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection"/> to an <see cref="Array"/>,
        /// starting at a particular <see cref="Array"/> index. 
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array">Array</see> that is the destination of the elements copied from the <see cref="ICollection"/>. The <see cref="Array">Array</see> must have zero-based indexing.</param> 
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
            if (array == null)
                throw new ArgumentNullException("array");

            // We must be careful not to corrupt the array, so we will first accumulate an 
            // internal list of elements that we will then copy to the array. This requires
            // some extra allocation, but is necessary since we don't know up front whether 
            // the array is sufficiently large to hold the stack's contents.
            ((ICollection) ToList()).CopyTo(array, index);
        }

        /// <summary>
        /// Copies the <see cref="ConcurrentDeque{T}"/> elements to an existing one-dimensional <see cref="Array">Array</see>, starting at the specified array index. 
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array">Array</see> that is the destination of the elements copied from the <see cref="ConcurrentDeque{T}"/>. The <see cref="Array">Array</see> must have zero-based indexing.
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
                throw new ArgumentNullException("array");

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

        /// <summary>
        /// Takes a moment-in-time snapshot of the deque.
        /// </summary>
        /// <returns>A list representing a moment-in-time snapshot of the deque.</returns>
        /// <remarks>
        /// The algorithm runs in linear O(n) time.
        /// 
        /// This implementation relies on the following invariant:
        /// If at time t, x was the leftmost node and y was the rightmost node, 
        /// regardless of how many nodes are pushed/popped from either end thereafter, the paths
        /// (a) x->a (obtained by traversing the deque recursively using a node's right pointer starting from x), and
        /// (b) y->b (obtained by traversing the deque recursively using a node's left pointer starting from y)
        /// will always have at least 1 node in common.
        /// 
        /// This means that, for a given x and y, even if the deque is mutated during the algorithm's
        /// execution, we can always rebuild the original x-y sequence by finding a node c, common to both
        /// x->a and y->b paths, and merging the paths by the common node.
        /// </remarks>
        private List<T> ToList()
        {
            //try to grab a reference to a stable anchor (fast route)
            Anchor anchor = _anchor;

            //try to grab a reference to a stable anchor (slow route)
            if (anchor._status != DequeStatus.Stable)
            {
                var spinner = new SpinWait();
                do
                {
                    anchor = _anchor;
                    spinner.SpinOnce();
                } while (anchor._status != DequeStatus.Stable);
            }

            var x = anchor._left;
            var y = anchor._right;

            //check if deque is empty
            if(x == null)
                return new List<T>();

            //check if deque has only 1 item
            if (x == y)
                return new List<T> {x._value};

            var xaPath = new List<Node>();
            var current = x;
            while (current != null && current != y)
            {
                xaPath.Add(current);
                current = current._right;
            }

            /*
             * If the 'y' node hasn't been popped from the right side of the deque,
             * then we should still be able to find the original x-y sequence 
             * using a node's right pointer.
             * 
             * If 'current' does not equal 'y', then the 'y' node must have been popped by
             * another thread during the while loop and the traversal wasn't successful.
             */
            if (current == y)
            {
                xaPath.Add(current);
                return xaPath.Select(node => node._value).ToList();
            }

            /*
             * If the 'y' node has been popped from the right end, we need to find all nodes that have
             * been popped from the right end and rebuild the original sequence.
             * 
             * To do this, we need to traverse the deque from right to left (using a node's left pointer)
             * until we find a node c common to both x->a and y->b paths. Such a node is either:
             * (a) currently part of the deque* or
             * (b) the last node of the x->a path (i.e., node 'a') or
             * (c) the last node to be popped from the left (if all nodes between 'x' and 'y' were popped from the deque).
             * 
             * -- Predicate (a) --
             * A node belongs to the deque if node.left.right == node (except for the leftmost node) if the deque has > 1 nodes.
             * If the deque has exactly one node, we know we've found that node if:
             * (1) all nodes to its right in the x-y sequence don't fall under predicates (a), (b) and (c) and
             * (2) node.left == null
             * 
             * -- Predicate (b) --
             * True for a node n if n == a
             * 
             * -- Predicate (c) --
             * True for a node n if:
             * (1) all nodes to its right in the x-y sequence don't fall under predicates (a), (b) and (c) and
             * (2) node.left == null
             */
            current = y;
            var a = xaPath.Last();
            var ycPath = new Stack<Node>();
            while (current._left != null &&
                   current._left._right != current &&
                   current != a)
            {
                ycPath.Push(current);
                current = current._left;
            }

            //this node is common to the list and the stack
            var common = current;
            ycPath.Push(common);

            /*
             * Merge the x->a and the y->c paths by the common node.
             * This is done by removing the nodes in x->a that come after c,
             * and appending all nodes in the y->c path in reverse order.
             * Since we used a LIFO stack to store all nodes in the y->c path,
             * we can simply iterate over it to reverse the order in which they were inserted.
             */
            var xySequence = xaPath
                .TakeWhile(node => node != common)
                .Select(node => node._value)
                .Concat(
                    ycPath.Select(node => node._value));

            return xySequence.ToList();
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
    }
}

#pragma warning restore 420
// ReSharper restore InconsistentNaming
