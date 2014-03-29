using System;
using System.Linq;
using System.Threading;
using DequeNet.Tests.Common;
using DequeNet.Tests.Helpers;
using Xunit;

namespace DequeNet.Tests.ConcurrentDeque.Internal
{
    public class StressTests
    {
        private const int ThreadCount = 20;
        private const int RunningTime = 3000;

        [Fact]
        [Trait("Category", "LongRunning")]
        public void PushRight_IsAtomic()
        {
            //Arrange
            long pushCount = 0;
            long sum = 0;
            bool cancelled = false;

            var deque = new ConcurrentDeque<int>();

            //keep adding items to the deque
            Action pushRight = () =>
            {
                Random rnd = new Random();
                while (!cancelled)
                {
                    int val = rnd.Next(1, 11);
                    deque.PushRight(val);
                    Interlocked.Increment(ref pushCount);
                    Interlocked.Add(ref sum, val);
                }
            };

            //Act
            pushRight.RunInParallel(() => cancelled = true, ThreadCount, RunningTime);

            //Assert
            VerifyState(deque, pushCount, sum);
        }

        [Fact]
        [Trait("Category", "LongRunning")]
        public void PushLeft_IsAtomic()
        {
            //Arrange
            long pushCount = 0;
            long sum = 0;
            bool cancelled = false;

            var deque = new ConcurrentDeque<int>();

            //keep adding items to the deque
            Action pushLeft = () =>
            {
                Random rnd = new Random();
                while (!cancelled)
                {
                    int val = rnd.Next(1, 11);
                    deque.PushLeft(val);
                    Interlocked.Increment(ref pushCount);
                    Interlocked.Add(ref sum, val);
                }
            };

            //Act
            pushLeft.RunInParallel(() => cancelled = true, ThreadCount, RunningTime);

            //Assert
            VerifyState(deque, pushCount, sum);
        }

        [Fact]
        [Trait("Category", "LongRunning")]
        public void TryPopRight_IsAtomic()
        {
            //Arrange
            const int initialCount = 5000000;
            const double stopAt = initialCount * 0.9;

            int popCount = 0;
            var deque = new ConcurrentDeque<int>();

            for (int i = 0; i < initialCount; i++)
                deque.PushRight(i);

            Action popRight = () =>
            {
                while (popCount <= stopAt)
                {
                    int i;
                    Assert.True(deque.TryPopRight(out i));
                    Interlocked.Increment(ref popCount);
                }
            };
            //Act
            popRight.RunInParallel(ThreadCount);

            //Assert
            var expectedCount = initialCount - popCount;
            long expectedSum = Enumerable.Range(0, expectedCount).LongSum();

            VerifyState(deque, expectedCount, expectedSum);
        }

        [Fact]
        [Trait("Category", "LongRunning")]
        public void TryPopLeft_IsAtomic()
        {
            //Arrange
            const int initialCount = 5000000;
            const double stopAt = initialCount * 0.9;

            int popCount = 0;
            var deque = new ConcurrentDeque<int>();

            for (int i = 0; i < initialCount; i++)
                deque.PushLeft(i);

            Action popLeft = () =>
            {
                while (popCount <= stopAt)
                {
                    int i;
                    Assert.True(deque.TryPopLeft(out i));
                    Interlocked.Increment(ref popCount);
                }
            };

            //Act
            popLeft.RunInParallel(ThreadCount);

            //Assert
            var expectedCount = initialCount - popCount;
            long expectedSum = Enumerable.Range(0, expectedCount).LongSum();

            VerifyState(deque, expectedCount, expectedSum);
        }

        /// <summary>
        /// Verifies that parallel interleaved push right/pop right operations don't leave the deque in a corrupted state
        /// </summary>
        [Fact]
        [Trait("Category", "LongRunning")]
        public void InterleavedPushPopRightOps()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            long sum = 0;
            long nodeCount = 0;
            bool cancelled = false;

            Action action = () =>
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
                        Interlocked.Increment(ref nodeCount);
                        Interlocked.Add(ref sum, val);
                    }
                    else
                    {
                        //pop
                        int val;
                        if (deque.TryPopRight(out val))
                        {
                            Interlocked.Decrement(ref nodeCount);
                            Interlocked.Add(ref sum, -val);
                        }
                    }
                }
            };

            //Act
            action.RunInParallel(() => cancelled = true, ThreadCount, RunningTime);

            //Assert
            VerifyState(deque, nodeCount, sum);
        }

        /// <summary>
        /// Verifies that parallel interleaved push right/pop left operations don't leave the deque in a corrupted state
        /// </summary>
        [Fact]
        [Trait("Category", "LongRunning")]
        public void InterleavedPushPopLeftOps()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            long sum = 0;
            long nodeCount = 0;
            bool cancelled = false;

            Action action = () =>
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
                        Interlocked.Increment(ref nodeCount);
                        Interlocked.Add(ref sum, val);
                    }
                    else
                    {
                        //pop
                        int val;
                        if (deque.TryPopLeft(out val))
                        {
                            Interlocked.Decrement(ref nodeCount);
                            Interlocked.Add(ref sum, -val);
                        }
                    }
                }
            };

            //Act
            action.RunInParallel(() => cancelled = true, ThreadCount, RunningTime);

            //Assert
            VerifyState(deque, nodeCount, sum);
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
        [Trait("Category", "LongRunning")]
        public void InterleavedOps()
        {
            //Arrange
            var deque = new ConcurrentDeque<int>();
            long sum = 0;
            long nodeCount = 0;
            bool cancelled = false;
            bool shouldPush = true;

            Action action = () =>
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
                        Interlocked.Increment(ref nodeCount);
                        Interlocked.Add(ref sum, val);

                        //start popping
                        if (nodeCount >= 10000)
                            shouldPush = false;
                    }
                    else
                    {
                        //pop from either end
                        int val;
                        if (rnd.NextDouble() > 0.50)
                        {
                            if (deque.TryPopLeft(out val))
                            {
                                Interlocked.Decrement(ref nodeCount);
                                Interlocked.Add(ref sum, -val);
                            }
                        }
                        else
                        {
                            if (deque.TryPopRight(out val))
                            {
                                Interlocked.Decrement(ref nodeCount);
                                Interlocked.Add(ref sum, -val);
                            }
                        }

                        //start pushing
                        if (nodeCount == 0)
                            shouldPush = true;
                    }
                }
            };

            //Act
            action.RunInParallel(() => cancelled = true, ThreadCount, RunningTime);

            //Assert
            VerifyState(deque, nodeCount, sum);
        }

        private void VerifyState(ConcurrentDeque<int> deque, long expectedCount, long expectedSum)
        {
            //Assert
            Assert.Equal(expectedCount, deque.Count);
            Assert.Equal(expectedSum, deque.LongSum());

            //test internal state
            //traverse the deque in both directions
            Assert.Equal(expectedCount, deque.TraverseLeftRight().LongCount());
            Assert.Equal(expectedCount, deque.TraverseRightLeft().LongCount());   
        }
    }
}
