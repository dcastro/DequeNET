using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DequeNet.Unit.Extensions
{
    public static class EnumerableExtensions
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
    }
}
