using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DequeNet
{
    public class ConcurrentDeque<T> : IConcurrentDeque<T>
    {
        private Anchor _anchor;

        public ConcurrentDeque()
        {
            _anchor = new Anchor();
        } 
        
        public void PushRight(T item)
        {
            throw new NotImplementedException();
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

        internal class Anchor
        {
            internal readonly Node Right;
            internal readonly Node Left;
            internal readonly DequeStatus Status;

            public Anchor()
            {
                Right = Left = null;
                Status = DequeStatus.Stable;
            }

            public Anchor(Node right, Node left, DequeStatus status)
            {
                Right = right;
                Left = left;
                Status = status;
            }
        }

        internal enum DequeStatus
        {
            Stable,
            RPush,
            LPush
        };

        internal class Node
        {
            internal Node Right;
            internal Node Left;
            internal readonly T Value;

            internal Node(T value)
            {
                Value = value;
            }
        }
    }
}
