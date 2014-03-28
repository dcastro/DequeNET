using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DequeNet.Tests.Common;

namespace DequeNet.Tests.Perf
{
    public static class Program
    {
        private const int RunningTime = 7000;
        private const int ThreadCount = 4;

        public static void Main(string[] args)
        {
            var deque = new ConcurrentDeque<int>(Enumerable.Repeat(1, 100000));
            bool cancelled = false;

            using (var countersContainer = CreateContainer(args))
            {
                Action action = () =>
                    {
                        var rnd = new Random(Thread.CurrentThread.ManagedThreadId);

                        while (!cancelled)
                        {
                            PerformRandomAction(deque, rnd);
                            countersContainer.Increment();
                        }
                    };

                Action cancel = () =>
                    {   
                        cancelled = true;
                        countersContainer.PrintCounters();
                    };

                action.RunInParallel(cancel, ThreadCount, RunningTime);
            }
        }

        private static IPerfCountersContainer CreateContainer(string[] args)
        {
            if (args.Any(arg => arg == "--with-slim-counters" ||
                                arg == "-wsc"))
                return new SlimPerfCountersContainer();

            if (args.Any(arg => arg == "--with-perf-counters" ||
                                arg == "-wpc"))
                return new PerfCountersContainer();

            return new NullCountersContainer();
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
