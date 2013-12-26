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
        private const int ThreadCount = 20;
        private const int RunningTime = 3000;

        /// <summary>
        /// Verifies that parallel interleaved push right/pop right operations don't leave the deque in a corrupted state
        /// </summary>
        [Fact]
        public void InterleavedPushPopRightOps()
        {
            //Arrange
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
            action.RunInParallel(() => cancelled = true, ThreadCount, RunningTime);

            //Assert
            long actualSum = deque.GetNodes().Sum(n => (long) n._value);
            Assert.Equal(sum, actualSum);
        }

        /// <summary>
        /// Verifies that parallel interleaved push right/pop left operations don't leave the deque in a corrupted state
        /// </summary>
        [Fact]
        public void InterleavedPushPopLeftOps()
        {
            //Arrange
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
                        deque.PushLeft(val);
                        Interlocked.Add(ref sum, val);
                    }
                    else
                    {
                        //pop
                        int val;
                        if (deque.TryPopLeft(out val))
                            Interlocked.Add(ref sum, -val);
                    }
                }
            };

            //Act
            action.RunInParallel(() => cancelled = true, ThreadCount, RunningTime);

            //Assert
            long actualSum = deque.GetNodes().Sum(n => (long)n._value);
            Assert.Equal(sum, actualSum);
        }

        /// <summary>
        /// Verifies that parallel interleaved push right/pop left/right operations don't leave the deque in a corrupted state.
        /// The test spins up 20 threads, each executing the same action.
        /// The threads start by pushing random integers on both ends of the deque until the total sum of all nodes reaches 5000.
        /// Then, they start popping items until the deque is empty, at which point they start pushing again, and so on, for 3 seconds.
        /// 
        /// At the end, we assert that we can still traverse the deque (from left to right, and right to left) and that the nodes contain the excepted values.
        /// </summary>
        [Fact]
        public void InterleavedOps()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            long sum = 0;
            bool cancelled = false;
            bool shouldPush = true;

            ThreadStart action = () =>
            {
                Random rnd = new Random();

                while (!cancelled)
                {
                    if (shouldPush)
                    {
                        //push to either end
                        var val = rnd.Next(1, 51);
                        if (rnd.NextDouble() > 0.50)
                            deque.PushLeft(val);
                        else
                            deque.PushRight(val);
                        Interlocked.Add(ref sum, val);

                        //start popping
                        if (sum >= 5000)
                            shouldPush = false;
                    }
                    else
                    {
                        //pop from either end
                        int val;
                        if (rnd.NextDouble() > 0.50)
                        {
                            if (deque.TryPopLeft(out val))
                                Interlocked.Add(ref sum, -val);
                        }
                        else
                        {
                            if (deque.TryPopRight(out val))
                                Interlocked.Add(ref sum, -val);
                        }

                        //start pushing
                        if (sum == 0)
                            shouldPush = true;
                    }
                }
            };

            //Act
            action.RunInParallel(() => cancelled = true, ThreadCount, RunningTime);

            //Assert
            long actualSum = deque.GetNodes().Sum(n => (long)n._value);
            Assert.Equal(sum, actualSum);

            actualSum = deque.GetNodesReverse().Sum(n => (long)n._value);
            Assert.Equal(sum, actualSum);
        }
    }
}
