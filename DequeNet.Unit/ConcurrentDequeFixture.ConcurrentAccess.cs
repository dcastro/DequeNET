using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DequeNet.Unit
{
    public partial class ConcurrentDequeFixture
    {
        // ReSharper disable AccessToModifiedClosure
        [Fact]
        public void ConcurrentPushRightMaintainsRightPointersIntegrity()
        {
            //Arrange
            long pushCount = 0;
            bool shouldPush = true;
            const int taskCount = 20;
            const int runningTime = 3000;

            var deque = new ConcurrentDeque<int>();

            //keep adding items to the deque
            Action pushRightAction = () =>
                                     {
                                         while (shouldPush)
                                         {
                                             deque.PushRight(0);
                                             Interlocked.Increment(ref pushCount);
                                         }
                                     };

            //Act
            //start concurrent tasks
            var tasks = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(pushRightAction);
            }

            //wait and stop tasks
            Thread.Sleep(runningTime);
            shouldPush = false;
            Task.WaitAll(tasks);

            //Assert
            //traverse the deque from left to right
            long nodesCount = 0;
            ForEachNode(deque, node => nodesCount++);

            Assert.Equal(pushCount, nodesCount);
        }

        [Fact]
        public void ConcurrentPushRightMaintainsLeftPointersIntegrity()
        {
            //Arrange
            long pushCount = 0;
            bool shouldPush = true;
            const int taskCount = 20;
            const int runningTime = 3000;

            var deque = new ConcurrentDeque<int>();

            //keep adding items to the deque
            Action pushRightAction = () =>
            {
                while (shouldPush)
                {
                    deque.PushRight(0);
                    Interlocked.Increment(ref pushCount);
                }
            };

            //Act
            //start concurrent tasks
            var tasks = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(pushRightAction);
            }

            //wait and stop tasks
            Thread.Sleep(runningTime);
            shouldPush = false;
            Task.WaitAll(tasks);

            //Assert
            //traverse the deque from right to left
            long nodesCount = 0;
            ReverseForEachNode(deque, node => nodesCount++);

            Assert.Equal(pushCount, nodesCount);
            Debug.WriteLine(pushCount);
        }

        [Fact]
        public void ConcurrentPushRightMaintainsValueIntegrity()
        {
            //Arrange
            long sum = 0;
            bool shouldPush = true;
            const int taskCount = 20;
            const int runningTime = 3000;

            var deque = new ConcurrentDeque<int>();

            //keep adding items to the deque
            Action pushRightAction = () =>
            {
                Random rnd = new Random();

                while (shouldPush)
                {
                    int val = rnd.Next(1, 11);
                    deque.PushRight(val);
                    Interlocked.Add(ref sum, val);
                }
            };

            //Act
            //start concurrent tasks
            var tasks = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(pushRightAction);
            }

            //wait and stop tasks
            Thread.Sleep(runningTime);
            shouldPush = false;
            Task.WaitAll(tasks);

            //Assert
            //traverse the deque from right to left
            long actualSum = 0;
            ReverseForEachNode(deque, node => actualSum += node.Value);
            
            Assert.Equal(sum, actualSum);
        }

        [Fact]
        public void TryPopRightIsAtomic()
        {
            //Arrange
            const int initialCount = 5000000;
            const double stopAt = initialCount*0.9;
            const int taskCount = 20;

            int popCount = 0;
            var deque = new ConcurrentDeque<int>();

            for (int i = 0; i < initialCount; i++)
                deque.PushRight(i);

            Action popRightAction = () =>
                                    {
                                        while (popCount <= stopAt)
                                        {
                                            int i;
                                            Assert.True(deque.TryPopRight(out i));
                                            Interlocked.Increment(ref popCount);
                                        }
                                    };
            //Act
            //start concurrent tasks
            var tasks = new Task[taskCount];
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = Task.Run(popRightAction);
            }

            //wait and stop tasks
            Task.WaitAll(tasks);

            //Assert
            int remainingNodes = 0;
            ForEachNode(deque, n => remainingNodes++);

            Assert.Equal(initialCount - popCount, remainingNodes);
        }
        // ReSharper enable AccessToModifiedClosure
    }
}
