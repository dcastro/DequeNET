using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DequeNet.Extensions;

namespace DequeNet
{
    /// <summary>
    /// Represents a double-ended queue, also known as deque (pronounced "deck").
    /// Items can be appended to/removed from both ends of the deque.
    /// </summary>
    /// <typeparam name="T">Specifies the type of the elements in the deque.</typeparam>
    public class Deque<T> : IDeque<T>
    {
        private const int DefaultCapacity = 4;

        private static readonly T[] EmptyBuffer = new T[0];

        /// <summary>
        /// Ring buffer that holds the items.
        /// </summary>
        private T[] _buffer;

        /// <summary>
        /// The offset used to calculate the position of the leftmost item in the buffer.
        /// </summary>
        private int _left;

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque{T}"/> class.
        /// </summary>
        public Deque()
        {
            _buffer = EmptyBuffer;
        }

        private int Left
        {
            get { return _left; }
            set { _left = ToIndex(value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque{T}"/> class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The deque's initial capacity.</param>
        /// <exception cref="ArgumentOutOfRangeException">Capacity cannot be less than 0.</exception>
        public Deque(int capacity)
        {
            if(capacity < 0)
                throw new ArgumentOutOfRangeException("capacity", "capacity was less than zero.");

            if (capacity == 0)
                _buffer = EmptyBuffer;
            else
                _buffer = new T[capacity];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Deque{T}"/> class that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the deque.</param>
        public Deque(IEnumerable<T> collection)
        {
            var capacity = collection.Count();
            if (capacity == 0)
                _buffer = EmptyBuffer;
            else
            {
                _buffer = new T[capacity];

                //copy to array
                if (collection is ICollection<T>)
                {
                    (collection as ICollection<T>).CopyTo(_buffer, 0);
                    Count = capacity;
                }
                else
                {
                    foreach (var item in collection)
                        PushRight(item);
                }
            }
        } 

        /// <summary>
        /// Gets a value that indicates whether the <see cref="Deque{T}"/> is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        /// <summary>
        /// Adds an item to the right end of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <param name="item">The item to be added to the <see cref="Deque{T}"/>.</param>
        public void PushRight(T item)
        {
            EnsureCapacity(Count + 1);

            //insert item
            var index = ToIndex(Left + Count);
            _buffer[index] = item;

            //inc count
            Count ++;
        }

        /// <summary>
        /// Adds an item to the left end of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <param name="item">The item to be added to the <see cref="Deque{T}"/>.</param>
        public void PushLeft(T item)
        {
            EnsureCapacity(Count + 1);

            //decrement left
            Left --;

            //insert item
            _buffer[Left] = item;

            //inc count
            Count ++;
        }

        /// <summary>
        /// Attempts to remove and return an item from the right end of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <returns>The rightmost item.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        public T PopRight()
        {
            if (IsEmpty)
                throw new InvalidOperationException("The deque is empty");

            //dec count
            Count--;

            //retrieve rightmost item
            var index = ToIndex(Left + Count);
            var item = _buffer[index];

            //clean reference
            _buffer[index] = default(T);

            return item;
        }

        /// <summary>
        /// Attempts to remove and return an item from the left end of the <see cref="Deque{T}"/>.
        /// </summary>
        /// <returns>The leftmost item.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        public T PopLeft()
        {
            if (IsEmpty)
                throw new InvalidOperationException("The deque is empty");

            //retrieve leftmost item
            var item = _buffer[Left];
            Count--;
            
            //clean reference
            _buffer[Left] = default(T);

            //increment left
            Left++;

            return item;
        }

        /// <summary>
        /// Attempts to return the rightmost item of the <see cref="Deque{T}"/> 
        /// without removing it.
        /// </summary>
        /// <returns>The rightmost item.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        public T PeekRight()
        {
            if (IsEmpty)
                throw new InvalidOperationException("The deque is empty");

            //retrieve rightmost item
            var index = ToIndex(Left + Count - 1);
            var item = _buffer[index];

            return item;
        }

        /// <summary>
        /// Attempts to return the leftmost item of the <see cref="Deque{T}"/> 
        /// without removing it.
        /// </summary>
        /// <returns>The leftmost item.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        public T PeekLeft()
        {
            if (IsEmpty)
                throw new InvalidOperationException("The deque is empty");

            //retrieve leftmost item
            return _buffer[Left];
        }

        /// <summary>
        /// Removes all items from the <see cref="Deque{T}"/>.
        /// </summary>
        public void Clear()
        {
            //clear the ring buffer to allow the GC to reclaim the references
            if (LoopsAround)
            {
                //clear both halves
                Array.Clear(_buffer, Left, Capacity - Left);
                Array.Clear(_buffer, 0, Left + (Count - Capacity));
            }
            else //clear the whole array
                Array.Clear(_buffer, Left, Count);

            Count = 0;
            Left = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the deque.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the deque.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {

            for (int i = 0; i < Count; i++)
            {
                var index = ToIndex(Left + i);
                yield return _buffer[index];
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a deque.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the deque.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds an item to the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
        /// <remarks>For <see cref="Deque{T}"/>, this operation will add the item to the right end of the deque.</remarks>
        void ICollection<T>.Add(T item)
        {
            PushRight(item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> was successfully removed from the <see cref="ICollection{T}"/>; otherwise, false.
        /// This method also returns false if <paramref name="item"/> is not found in the original <see cref="ICollection{T}"/>.
        /// </returns>
        /// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
        bool ICollection<T>.Remove(T item)
        {
            //find the index of the item to be removed in the deque
            var comp = EqualityComparer<T>.Default;
            int dequeIndex = -1;
            int counter = 0;
            foreach (var dequeItem in this)
            {
                if (comp.Equals(item, dequeItem))
                {
                    dequeIndex = counter;
                    break;
                }
                counter++;
            }

            //return false if the item wasn't found
            if (dequeIndex == -1)
                return false;

            //if the removal should be performed on one of the ends, use the corresponding Pop operation instead
            if (dequeIndex == 0)
                PopLeft();
            else if (dequeIndex == Count - 1)
                PopRight();
            else
            {
                if (dequeIndex < Count/2) //If the item is located towards the left end of the deque
                {
                    //move the items to the left of 'item' one index to the right
                    for (int i = dequeIndex - 1; i >= 0; i--)
                    {
                        //calculate array indexes
                        int sourceIndex = ToIndex(Left + i);
                        int destinationIndex = ToIndex(sourceIndex + 1);

                        _buffer[destinationIndex] = _buffer[sourceIndex];
                    }

                    //clean first item
                    _buffer[Left] = default(T);

                    //increase left
                    Left ++;
                    Count --;
                }
                else //If the item is located towards the right end of the deque
                {
                    //move the items to the right of 'item' one index to the left
                    for (int i = dequeIndex + 1; i < Count; i++)
                    {
                        //calculate array indexes
                        int sourceIndex = ToIndex(Left + i);
                        int destinationIndex = ToIndex(sourceIndex - 1);

                        _buffer[destinationIndex] = _buffer[sourceIndex];
                    }

                    //clean last item
                    var lastItemIndex = ToIndex(Left + Count - 1);
                    _buffer[lastItemIndex] = default(T);

                    //decrease count
                    Count--;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the <see cref="Deque{T}"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if <paramref name="item"/> is found in the <see cref="Deque{T}"/>; otherwise, false.
        /// </returns>
        /// <param name="item">The object to locate in the <see cref="Deque{T}"/>.</param>
        public bool Contains(T item)
        {
            return Enumerable.Contains(this, item, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Copies the elements of the <see cref="Deque{T}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="Deque{T}"/>. The <see cref="Array"/> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0 or greater than the <paramref name="array"/>'s upper bound.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source <see cref="Deque{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if(array == null)
                throw new ArgumentNullException("array");

            if(arrayIndex < 0)
                throw new ArgumentOutOfRangeException("arrayIndex", "Index was less than the array's lower bound.");

            if (arrayIndex >= array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex", "Index was greater than the array's upper bound.");

            if (Count == 0)
                return;

            if(array.Length - arrayIndex < Count)
                throw new ArgumentException("Destination array was not long enough");

            //if the elements are stored sequentially (i.e., left+count doesn't "loop around" the array's boundary),
            //copy the array as a whole
            if (!LoopsAround)
            {
                Array.Copy(_buffer, Left, array, arrayIndex, Count);
            }
            else
            {
                //copy both halves to a new array
                Array.Copy(_buffer, Left, array, arrayIndex, Capacity - Left);
                Array.Copy(_buffer, 0, array, arrayIndex + Capacity - Left, Left + (Count - Capacity));
            }
        }

        /// <summary> 
        /// Ensures that the capacity of this list is at least the given minimum
        /// value. If the currect capacity of the list is less than min, the
        /// capacity is increased to twice the current capacity or to min,
        /// whichever is larger.
        /// </summary>
        /// <param name="min">The minimum capacity required.</param>
        private void EnsureCapacity(int min)
        {
            if (Capacity < min)
            {
                var newCapacity = Capacity == 0 ? DefaultCapacity : Capacity*2;
                newCapacity = Math.Max(newCapacity, min);
                Capacity = newCapacity;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="Deque{T}"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="Deque{T}"/>.
        /// </returns>
        public int Count { get; private set; }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can hold without resizing.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="Capacity"/> cannot be set to a value less than <see cref="Count"/>.</exception>
        public int Capacity
        {
            get { return _buffer.Length; }
            set
            {
                var count = Count;
                if (value < count)
                    throw new ArgumentOutOfRangeException("value", "capacity was less than the current size.");

                if (value == Capacity)
                    return;

                T[] newBuffer = new T[value];

                CopyTo(newBuffer, 0);

                Left = 0;
                _buffer = newBuffer;
            }
        }

        /// <summary>
        /// Determines whether the deque "loops around" the array's boundary, i.e., whether the rightmost's index is lower than the leftmost's.
        /// </summary>
        /// <returns>true if the deque loops around the array's boundary; false otherwise.</returns>
        public bool LoopsAround
        {
            get
            {
                return Count > (Capacity - Left);
            }
        }

        /// <summary>
        /// Uses modular arithmetic to calculate the correct ring buffer index for a given index.
        /// If <paramref name="position"/> is over the array's upper boundary, the returned index "wraps/loops around" the upper boundary.
        /// </summary>
        /// <param name="position">The index.</param>
        /// <returns>The ring buffer index.</returns>
        public int ToIndex(int position)
        {
            //put 'position' in the range [0, Capacity-1] using modular arithmetic
            if (Capacity != 0)
                return position.Mod(Capacity);

            //if capacity is 0, _left must always be 0
            Contract.Assert(_left == 0);

            return 0;
        }
    }
}
