using System;
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
        internal Anchor _anchor;

        public ConcurrentDeque()
        {
            _anchor = new Anchor();
        } 
        
        public void PushRight(T item)
        {
            var newNode = new Node(item);

            while (true)
            {
                var anchor = _anchor;

                //If the deque is empty
                if (anchor.Right == null)
                {
                    //update both pointers to point to the new node
                    var newAnchor = new Anchor(newNode, newNode, anchor.Status);

                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        return;
                }
                else if (anchor.Status == DequeStatus.Stable)
                {
                    //update right pointer
                    //and change the status to RPush
                    newNode.Left = anchor.Right;
                    var newAnchor = new Anchor(anchor.Left, newNode, DequeStatus.RPush);

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
            throw new NotImplementedException();
        }

        public bool TryPopRight(out T item)
        {
            Anchor anchor;
            while (true)
            {
                anchor = _anchor;
                
                if (anchor.Right == null)
                {
                    //return false if the deque is empty
                    item = default(T);
                    return false;
                }
                if (anchor.Right == anchor.Left)
                {
                    //update both pointers if the deque has only one node
                    var newAnchor = new Anchor(null, null, DequeStatus.Stable);
                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        break;
                }
                else if (anchor.Status == DequeStatus.Stable)
                {
                    //update right pointer if deque has > 1 node
                    var prev = anchor.Right.Left;
                    var newAnchor = new Anchor(anchor.Left, prev, anchor.Status);
                    if (Interlocked.CompareExchange(ref _anchor, newAnchor, anchor) == anchor)
                        break;
                }
                else
                {
                    //if the deque is unstable,
                    //attempt to bring it to a stable state before trying to insert the node.
                    Stabilize(anchor);
                }
            }

            item = anchor.Right.Value;
            return true;
        }

        public bool TryPopLeft(out T item)
        {
            throw new NotImplementedException();
        }

        private void Stabilize(Anchor anchor)
        {
            if(anchor.Status == DequeStatus.RPush)
                StabilizeRight(anchor);
            else
                StabilizeLeft(anchor);
        }

        private void StabilizeLeft(Anchor anchor)
        {

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
            var newNode = anchor.Right;

            //grab a reference to the previous rightmost node and its right pointer
            var prev = newNode.Left;
            var prevNext = prev.Right;

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
                if (Interlocked.CompareExchange(ref prev.Right, newNode, prevNext) != prevNext)
                    return;
            }

            /**
             * Try to mark the anchor as stable.
             * This step is done outside of the previous "if" block:
             *   even though another thread may have already updated prev's right pointer,
             *   this thread might still preempt the other and perform the second step (i.e., update the anchor).
             */
            var newAnchor = new Anchor(anchor.Left, anchor.Right, DequeStatus.Stable);
            Interlocked.CompareExchange(ref _anchor, newAnchor, anchor);

        }

        internal class Anchor
        {
            internal readonly Node Left;
            internal readonly Node Right;
            internal readonly DequeStatus Status;

            public Anchor()
            {
                Right = Left = null;
                Status = DequeStatus.Stable;
            }

            public Anchor(Node left, Node right, DequeStatus status)
            {
                Left = left;
                Right = right;
                Status = status;
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
            internal Node Left;
            internal Node Right;
            internal readonly T Value;

            internal Node(T value)
            {
                Value = value;
            }
        }
    }
}
