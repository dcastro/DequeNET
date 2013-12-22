using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace DequeNet.Functional
{
    public class ConcurrentDequeFixture
    {
        /// <summary>
        /// Verifies that parallel interleaved push right/pop right operations don't leave the deque in a corrupted state
        /// </summary>
        [Fact]
        public void InterleavedPushPopRightOps()
        {
            //Arrange
            const int threadCount = 20;
            const int runningTime = 3000;
            const int threadTimeout = 500;

            var deque = new ConcurrentDeque<int>();
            long sum = 0;
            bool cancelled = false;

            ThreadStart action = () =>
                            {
                                Random rnd = new Random();

                                while (!cancelled)
                                {
                                    //slightly biased towards "push"
                                    if (rnd.NextDouble() >= 0.45)
                                    {
                                        //push
                                        var val = rnd.Next(1, 51);
                                        deque.PushRight(val);
                                        Interlocked.Add(ref sum, val);
                                    }
                                    else
                                    {
                                        //pop
                                        int val;
                                        if (deque.TryPopRight(out val))
                                            Interlocked.Add(ref sum, - val);
                                    }
                                }
                            };

            //Act
            //start concurrent threads
            var threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(action);
                threads[i].Start();
            }

            Thread.Sleep(runningTime);
            cancelled = true;
            for (int i = 0; i < threadCount; i++)
            {
                Assert.True(threads[i].Join(threadTimeout));
            }

            //Assert
            long actualSum = GetNodes(deque).Sum(n => (long) n.Value);
            Assert.Equal(sum, actualSum);
        }

        private IEnumerable<ConcurrentDeque<T>.Node> GetNodes<T>(ConcurrentDeque<T> deque)
        {
            var anchor = deque._anchor;
            var current = anchor.Left;
            var last = anchor.Right;

            if (current == null)
                yield break;

            while (current != last)
            {
                yield return current;
                current = current.Right;
            }
            yield return last;
        }
    }
}
