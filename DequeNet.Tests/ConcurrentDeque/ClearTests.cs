using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace DequeNet.Tests.ConcurrentDeque
{
    public class ClearTests
    {
        [Fact]
        public void ClearRemovesAllItems()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>(new[] {1, 2, 3, 4});

            //Act
            deque.Clear();

            //Assert
            Assert.True(deque.IsEmpty);
        }
    }
}
