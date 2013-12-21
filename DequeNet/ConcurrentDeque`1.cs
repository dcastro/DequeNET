using System;
using System.Collections.Generic;
using System.Linq;
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
            throw new NotImplementedException();
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

        private void StabilizeRight(Anchor anchor)
        {
            
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
