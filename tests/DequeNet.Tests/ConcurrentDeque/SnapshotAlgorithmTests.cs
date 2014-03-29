using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DequeNet.Tests.Common;
using Xunit;
using Xunit.Extensions;

namespace DequeNet.Tests.ConcurrentDeque
{
    public class SnapshotAlgorithmTests
    {
        private const int ThreadCount = 10;

        private const int PopLeft = 0;
        private const int PopRight = 1;
        private const int PushLeft = 2;
        private const int PushRight = 3;

        public static IEnumerable<object[]> MutationSteps
        {
            get
            {
                return new[]
                    {
                        new object[] {"No-op", new int[] {}},
                        new object[] {"PopLeft all nodes", new[] {PopLeft, PopLeft, PopLeft, PopLeft, PopLeft}},
                        new object[] {"PopRight all nodes", new[] {PopRight, PopRight, PopRight, PopRight, PopRight}},
                        new object[] {"PopRight all nodes, PushRight once", new[] {PopRight, PopRight, PopRight, PopRight, PopRight, PushRight}},
                        new object[] {"Pop all nodes", new[] {PopRight, PopRight, PopLeft, PopLeft, PopLeft}},
                        new object[] {"Pop all but one node", new[] {PopRight, PopRight, PopLeft, PopLeft}},
                        new object[] {"Pop once from both ends", new[] {PopRight, PopLeft}},
                        new object[] {"PopRight once, PushRight once", new[] {PopRight, PushRight}},
                        new object[] {"PopRight twice, PushRight once", new[] {PopRight, PopRight, PushRight}},
                        new object[] {"PopRight, PushRight, PopLeft all x-y nodes", new[] {PopRight, PushRight, PopLeft, PopLeft, PopLeft, PopLeft}},
                        new object[] {"PopRight, PushRight, PopLeft all nodes", new[] {PopRight, PushRight, PopLeft, PopLeft, PopLeft, PopLeft, PopLeft}},
                        new object[] {"PopLeft, PushLeft, PopRight all x-y nodes", new[] {PopLeft, PushLeft, PopRight, PopRight, PopRight, PopRight}},
                        new object[] {"PopLeft, PushLeft, PopRight all nodes", new[] {PopLeft, PushLeft, PopRight, PopRight, PopRight, PopRight, PopRight}},
                        new object[] {"Pop/Push right, Pop/Push left", new[] {PopRight, PushRight, PopLeft, PushLeft}}
                    };
            }
        }
        
        [Theory]
        [PropertyData("MutationSteps")]
        public void Algorithm_Recreates_OriginalXySequence(string msg, IEnumerable<int> ops)
        {
            int[] array = {0, 1, 2, 3, 4};
            var deque = new ConcurrentDeque<int>(array);

            Action<ConcurrentDeque<int>> mutationCallback = d =>
                {
                    foreach (var op in ops)
                        ExecuteOp(deque, op);
                };

            var snapshot = Execute(deque, mutationCallback);

            Assert.Equal(array, snapshot);
        }

        [RepeatTest(30)]
        [Trait("Category", "LongRunning")]
        public void Algorithm_Recreates_OriginalXySequence_StressTest()
        {
            int[] array = {0, 1, 2, 3, 4};
            var deque = new ConcurrentDeque<int>(array);

            bool cancelled = false;
            Thread[] threads = null;
            Action<ConcurrentDeque<int>> mutationCallback = d =>
                {
                    Action executeOps = () =>
                        {
                            var rnd = new Random(Thread.CurrentThread.ManagedThreadId);

                            //randomly mutate deque
                            while (!cancelled)
                                ExecuteOp(deque, rnd.Next(4));
                        };

                    threads = executeOps.StartInParallel(ThreadCount);

                    //yield the processor and let the threads run
                    Thread.Yield();
                };

            //Act
            var snapshot = Execute(deque, mutationCallback);

            //stop threads
            cancelled = true;
            Assert.NotNull(threads);
            foreach (var thread in threads)
                thread.Join();

            //Assert
            Assert.Equal(array, snapshot);
        }

        private static List<T> Execute<T>(ConcurrentDeque<T> deque, Action<ConcurrentDeque<T>> mutationCallback)
        {
            //try to grab a reference to a stable anchor (fast route)
            ConcurrentDeque<T>.Anchor anchor = deque._anchor;

            //try to grab a reference to a stable anchor (slow route)
            if (anchor._status != ConcurrentDeque<T>.DequeStatus.Stable)
            {
                var spinner = new SpinWait();
                do
                {
                    anchor = deque._anchor;
                    spinner.SpinOnce();
                } while (anchor._status != ConcurrentDeque<T>.DequeStatus.Stable);
            }

            var x = anchor._left;
            var y = anchor._right;

            //run callback
            mutationCallback(deque);

            if (x == null)
                return new List<T>();

            if (x == y)
                return new List<T> {x._value};

            var xaPath = new List<ConcurrentDeque<T>.Node>();
            var current = x;
            while (current != null && current != y)
            {
                xaPath.Add(current);
                current = current._right;
            }

            if (current == y)
            {
                xaPath.Add(current);
                return xaPath.Select(node => node._value).ToList();
            }

            current = y;
            var a = xaPath.Last();
            var ycPath = new Stack<ConcurrentDeque<T>.Node>();
            while (current._left != null &&
                   current._left._right != current &&
                   current != a)
            {
                ycPath.Push(current);
                current = current._left;
            }

            var common = current;
            ycPath.Push(common);

            var xySequence = xaPath
                .TakeWhile(node => node != common)
                .Select(node => node._value)
                .Concat(
                    ycPath.Select(node => node._value));

            return xySequence.ToList();
        }

        private static void ExecuteOp(ConcurrentDeque<int> deque, int op)
        {
            int item;
            switch (op)
            {
                case PopLeft:
                    deque.TryPopLeft(out item);
                    break;
                case PopRight:
                    deque.TryPopRight(out item);
                    break;
                case PushLeft:
                    deque.PushLeft(10);
                    break;
                case PushRight:
                    deque.PushRight(10);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
