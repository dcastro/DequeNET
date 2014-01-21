using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Tests.Deque
{
    public class SerializationTests
    {
        [Fact]
        public void CanBeSerialized()
        {
            //Arrange
            var array = new[] { 1, 2, 3, 4 };
            var deque = new Deque<int>(array);

            //make deque "wrap around" the inner ring buffer
            deque.PopLeft();
            deque.PopLeft();
            deque.PushRight(2);

            Deque<int> deserializedDeque = null;

            //Act & Assert
            using (var ms = new MemoryStream())
            {
                //serialize
                var formatter = new BinaryFormatter();
                Assert.DoesNotThrow(() => formatter.Serialize(ms, deque));

                //deserialize
                ms.Seek(0, SeekOrigin.Begin);
                Assert.DoesNotThrow(() => deserializedDeque = formatter.Deserialize(ms) as Deque<int>);
            }

            //assert equivalence
            Assert.NotNull(deserializedDeque);
            Assert.Equal(deque, deserializedDeque);
            Assert.Equal(deque.Count, deserializedDeque.Count);
            Assert.Equal(deque.Capacity, deserializedDeque.Capacity);
        }
    }
}
