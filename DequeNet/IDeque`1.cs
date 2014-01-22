using System;
using System.Collections.Generic;

namespace DequeNet
{
    /// <summary>
    /// Represents a double-ended queue, also known as deque (pronounced "deck").
    /// Items can be appended to/removed from both ends of the deque.
    /// </summary>
    /// <typeparam name="T">Specifies the type of the elements in the deque.</typeparam>
    public interface IDeque<T> : ICollection<T>
    {
        /// <summary>
        /// Gets a value that indicates whether the <see cref="IDeque{T}"/> is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Adds an item to the right end of the <see cref="IDeque{T}"/>.
        /// </summary>
        /// <param name="item">The item to be added to the <see cref="IDeque{T}"/>.</param>
        void PushRight(T item);

        /// <summary>
        /// Adds an item to the left end of the <see cref="IDeque{T}"/>.
        /// </summary>
        /// <param name="item">The item to be added to the <see cref="IDeque{T}"/>.</param>
        void PushLeft(T item);

        /// <summary>
        /// Attempts to remove and return an item from the right end of the <see cref="IDeque{T}"/>.
        /// </summary>
        /// <returns>The rightmost item.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        T PopRight();

        /// <summary>
        /// Attempts to remove and return an item from the left end of the <see cref="IDeque{T}"/>.
        /// </summary>
        /// <returns>The leftmost item.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        T PopLeft();

        /// <summary>
        /// Attempts to return the rightmost item of the <see cref="IDeque{T}"/> 
        /// without removing it.
        /// </summary>
        /// <returns>The rightmost item.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        T PeekRight();

        /// <summary>
        /// Attempts to return the leftmost item of the <see cref="IDeque{T}"/> 
        /// without removing it.
        /// </summary>
        /// <returns>The leftmost item.</returns>
        /// <exception cref="InvalidOperationException">The deque is empty.</exception>
        T PeekLeft();

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Index was out of range. Must be non-negative and less than <see cref="ICollection{T}.Count"/>.</exception>
        T this[int index] { get; set; }
    }
}
