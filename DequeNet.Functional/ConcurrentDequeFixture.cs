using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;
using DequeNet.Test.Common;

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
            action.RunInParallel(() => cancelled = true, threadCount, runningTime);

            //Assert
            long actualSum = deque.GetNodes().Sum(n => (long) n._value);
            Assert.Equal(sum, actualSum);
        }
    }
}
