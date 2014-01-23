using System;
using Xunit;

namespace DequeNet.Tests.Helpers
{
    public static class ArrayExtensions
    {
        public static void AssertNotCorrupted<T>(this Array array)
        {
            var defaultValue = default(T);

            foreach (var item in array)
                Assert.Equal(defaultValue, item);
        }

        public static void AssertNotCorrupted<T>(this T[] array)
        {
            (array as Array).AssertNotCorrupted<T>();
        }
    }
}
