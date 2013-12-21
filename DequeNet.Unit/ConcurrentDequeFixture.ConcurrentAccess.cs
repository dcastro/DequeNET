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

        private void ForEachNode<T>(ConcurrentDeque<T> deque, Action<ConcurrentDeque<T>.Node> action)
        {
            var anchor = deque._anchor;
            var current = anchor.Left;
            var last = anchor.Right;

            while (current != last)
            {
                action(current);
                current = current.Right;
            }
            action(last);
        }

        private void ReverseForEachNode<T>(ConcurrentDeque<T> deque, Action<ConcurrentDeque<T>.Node> action)
        {
            var anchor = deque._anchor;
            var current = anchor.Right;
            var first = anchor.Left;

            while (current != first)
            {
                action(current);
                current = current.Left;
            }
            action(first);
        }
        // ReSharper enable AccessToModifiedClosure
    }
}
