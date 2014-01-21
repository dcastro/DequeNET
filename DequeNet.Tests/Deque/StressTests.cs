using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace DequeNet.Tests.Deque
{
    public class StressTests
    {
        private List<int> _shadow;
        private Deque<int> _deque;
        private int _previousCapacity;

        private enum Op
        {
            PopRight,
            PopLeft,
            PushRight,
            PushLeft,
            Remove,
            Add
        }

        [Fact]
        public void StressTest()
        {
            _shadow = new List<int>(new[] {1, 2, 3, 4});
            _deque = new Deque<int>(new[] {1, 2, 3, 4});

            var generator = new RandomOpGenerator();
            var rnd = new Random();
            const int operationsCount = 100000;

            for (int i = 0; i < operationsCount; i++)
            {
                //mutate deque
                if (i%300 == 0)
                {
                    //clear every 300 mutations
                    _deque.Clear();
                    _shadow.Clear();
                }
                else if (i%200 == 0)
                {
                    //trim excess every 300 mutations
                    _deque.TrimExcess();
                    _shadow.TrimExcess();
                    Assert.Equal(_deque.Count, _deque.Capacity);
                }
                else if (i%100 == 0)
                {
                    //serialize/deserialize deque every 300 mutations
                    using (var ms = new MemoryStream())
                    {
                        //serialize
                        var formatter = new BinaryFormatter();
                        formatter.Serialize(ms, _deque);

                        //deserialize
                        ms.Seek(0, SeekOrigin.Begin);
                        var tempDeque = formatter.Deserialize(ms) as Deque<int>;

                        Assert.NotNull(tempDeque);
                        Assert.Equal(_deque.Capacity, tempDeque.Capacity);

                        //replace _deque with the deserialized deque
                        _deque = tempDeque;
                    }
                }
                else
                {
                    //draw a random operation
                    var op = generator.Pick();
                    int val = rnd.Next(1000);

                    switch (op)
                    {
                        case Op.PushRight:
                            val = rnd.Next(1000);
                            _deque.PushRight(val);
                            _shadow.Add(val);
                            break;
                        case Op.PushLeft:
                            val = rnd.Next(1000);
                            _deque.PushLeft(val);
                            _shadow.Insert(0, val);
                            break;
                        case Op.Add:
                            val = rnd.Next(1000);
                            (_deque as ICollection<int>).Add(val);
                            _shadow.Add(val);
                            break;
                        case Op.PopLeft:
                            if (_deque.IsEmpty)
                                goto case Op.PushLeft;
                            Assert.DoesNotThrow(() => val = _deque.PopLeft());
                            Assert.Equal(_shadow.First(), val);
                            _shadow.RemoveAt(0);
                            break;
                        case Op.PopRight:
                            if (_deque.IsEmpty)
                                goto case Op.PushRight;

                            Assert.DoesNotThrow(() => val = _deque.PopRight());
                            Assert.Equal(_shadow.Last(), val);
                            _shadow.RemoveAt(_shadow.Count - 1);
                            break;
                        case Op.Remove:
                            if (_deque.IsEmpty)
                                goto case Op.Add;

                            //draw a random item
                            int index = rnd.Next(_shadow.Count);
                            int item = _shadow[index];
                            Assert.True((_deque as ICollection<int>).Remove(item));
                            _shadow.Remove(item);
                            break;
                    }
                }
                VerifyEmpty();
                VerifyCount();
                VerifySequence();
                VerifyEnds();
                VerifyCapacity();
            }
        }

        private void VerifyCapacity()
        {
            //assert that the capacity is never raised to more than double the number of elements in the deque
            if (_deque.Capacity != _previousCapacity)
            {
                Assert.True(_deque.Capacity <= 4 || _deque.Capacity <= 2*_deque.Count);
                _previousCapacity = _deque.Capacity;
            }
        }

        private void VerifyEmpty()
        {
            if (_shadow.Count == 0)
                Assert.True(_deque.IsEmpty);
        }

        private void VerifyCount()
        {
            Assert.Equal(_shadow.Count, _deque.Count);
        }

        private void VerifySequence()
        {
            Assert.Equal(_shadow, _deque);
        }

        private void VerifyEnds()
        {
            if (_shadow.Count != 0)
            {
                Assert.Equal(_shadow.First(), _deque.PeekLeft());
                Assert.Equal(_shadow.Last(), _deque.PeekRight());
            }
            else
            {
                Assert.Throws<InvalidOperationException>(() => _deque.PeekLeft());
                Assert.Throws<InvalidOperationException>(() => _deque.PeekRight());
                Assert.Throws<InvalidOperationException>(() => _deque.PopLeft());
                Assert.Throws<InvalidOperationException>(() => _deque.PopRight());
            }
        }
        private class RandomOpGenerator
        {
            private readonly List<Op> _distribution;
            private readonly Random _rnd;
            public RandomOpGenerator()
            {
                //dictionary of <operation, weight> pairs
                //slightly biased towards "push" operations
                var choices = new SortedDictionary<Op, int>
                {
                    {Op.PushRight, 2},
                    {Op.PushLeft, 2},
                    {Op.Add, 1},

                    {Op.PopRight, 1},
                    {Op.PopLeft, 1},
                    {Op.Remove, 1}                    
                };

                _distribution = choices
                    .SelectMany(kv =>
                        Enumerable.Repeat(kv.Key, kv.Value))
                    .ToList();

                _rnd = new Random();
            }

            public Op Pick()
            {
                int index = _rnd.Next(_distribution.Count);
                return _distribution[index];
            }
        }
    }
}
