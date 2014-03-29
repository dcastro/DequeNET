using System;
using System.Linq;

namespace DequeNet.Tests.Helpers
{
    internal static class ArrayExtensions
    {
        /// <summary>
        /// Determines whether all elements of a sequence are set to the default value of <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static bool AllDefault<T>(this Array array)
        {
            return array.Cast<T>().AllDefault();
        }
    }
}
