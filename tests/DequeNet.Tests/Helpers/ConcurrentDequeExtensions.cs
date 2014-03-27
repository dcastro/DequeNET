using System.Collections.Generic;

namespace DequeNet.Tests.Helpers
{
    internal static class ConcurrentDequeExtensions
    {
        public static IEnumerable<ConcurrentDeque<T>.Node> TraverseLeftRight<T>(this ConcurrentDeque<T> deque)
        {
            var anchor = deque._anchor;
            var current = anchor._left;
            var last = anchor._right;

            if (current == null)
                yield break;

            while (current != last)
            {
                yield return current;
                current = current._right;
            }
            yield return last;
        }

        public static IEnumerable<ConcurrentDeque<T>.Node> TraverseRightLeft<T>(this ConcurrentDeque<T> deque)
        {
            var anchor = deque._anchor;
            var current = anchor._right;
            var first = anchor._left;

            if (current == null)
                yield break;

            while (current != first)
            {
                yield return current;
                current = current._left;
            }
            yield return first;
        }
    }
}
