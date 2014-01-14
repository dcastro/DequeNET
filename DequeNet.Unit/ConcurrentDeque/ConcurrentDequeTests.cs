using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;
using DequeNet.Test.Common;
using Xunit.Extensions;

namespace DequeNet.Unit.ConcurrentDeque
{
    public partial class ConcurrentDequeTests
    {
        [Fact]
        public void CanBeSerialized()
        {
            //Arrange
            var array = new[] {1, 2, 3};
            var deque = new ConcurrentDeque<int>(array);

            ConcurrentDeque<int> deserializedDeque = null;

            //Act & Assert
            using (var ms = new MemoryStream())
            {
                //serialize
                var formatter = new BinaryFormatter();
                Assert.DoesNotThrow(() => formatter.Serialize(ms, deque));

                //deserialize
                ms.Seek(0, SeekOrigin.Begin);
                Assert.DoesNotThrow(() => deserializedDeque = formatter.Deserialize(ms) as ConcurrentDeque<int>);
            }

            //assert equivalence
            Assert.NotNull(deserializedDeque);
            Assert.Equal(array, deserializedDeque);
        }

        [Fact]
        public void ClearRemovesAllItems()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            deque.PushLeft(5);

            //Act
            deque.Clear();

            //Assert
            Assert.True(deque.IsEmpty);
        }
    }
}
