using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DequeNet.Extensions;
using Xunit;
using Xunit.Extensions;

namespace DequeNet.Unit.Extensions
{
    public class IntExtensionsTests
    {
        [Theory]
        [PropertyData("GetTestData")]
        public void Mod_ReturnsCorrectModulo(int dividend, int divisor, int expectedMod)
        {
            Assert.Equal(expectedMod, dividend.Mod(divisor));
        }

        [Fact]
        public void Mod_ThrowsException_IfDivisorIsZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => 1.Mod(0));
        }

        public static IEnumerable<object[]> GetTestData
        {
            get
            {
                yield return new object[] {1, 1, 0};
                yield return new object[] {0, 1, 0};
                yield return new object[] {2, 10, 2};
                yield return new object[] {12, 10, 2};
                yield return new object[] {22, 10, 2};
                yield return new object[] {-2, 10, 8};
                yield return new object[] {-12, 10, 8};
                yield return new object[] {-22, 10, 8};
                yield return new object[] { 2, -10, -8 };
                yield return new object[] { 12, -10, -8 };
                yield return new object[] { 22, -10, -8 };
                yield return new object[] { -2, -10, -2 };
                yield return new object[] { -12, -10, -2 };
                yield return new object[] { -22, -10, -2 };
            }
        }
    }
}
