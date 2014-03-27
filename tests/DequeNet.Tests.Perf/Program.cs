using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DequeNet.Tests.Common;

namespace DequeNet.Tests.Perf
{
    public static class Program
    {
        private const int RunningTime = 5000;
        private const int ThreadCount = 4;

        public static void Main(string[] args)
        {
            var deque = new ConcurrentDeque<int>(Enumerable.Repeat(1, 100000));
            bool cancelled = false;

            Action action = () =>
                {
                    var rnd = new Random(Thread.CurrentThread.ManagedThreadId);

                    while (!cancelled)
                    {
                        PerformRandomAction(deque, rnd);
                    }
                };

            action.RunInParallel(() => cancelled = true, ThreadCount, RunningTime);
        }

        private static void PerformRandomAction(IConcurrentDeque<int> deque, Random rnd)
        {
            int randomOp = rnd.Next(4);
            int poppedValue;

            switch (randomOp)
            {
                case 0:
                    deque.PushLeft(1);
                    break;
                case 1:
                    deque.PushRight(1);
                    break;
                case 2:
                    deque.TryPopLeft(out poppedValue);
                    break;
                case 3:
                    deque.TryPopRight(out poppedValue);
                    break;
            }
        }
    }
}
