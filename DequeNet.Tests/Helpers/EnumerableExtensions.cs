using System;
using System.Collections.Generic;
using System.Linq;

namespace DequeNet.Tests.Helpers
{
    internal static class EnumerableExtensions
    {
        public static long LongSum<T>(this IEnumerable<T> collection, Func<T, int> selector)
        {
            long sum = 0;
            foreach (var i in collection)
                sum += selector(i);

            return sum;
        }

        public static long LongSum(this IEnumerable<int> collection)
        {
            long sum = 0;
            foreach (var i in collection)
                sum += i;

            return sum;
        }
        
        /// <summary>
        /// Determines whether all elements of a sequence are set to the default value of <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool AllDefault<T>(this IEnumerable<T> array)
        {
            var comparer = EqualityComparer<T>.Default;
            var defaultValue = default(T);

            return array.All(item => comparer.Equals(item, defaultValue));
        }
    }
}
