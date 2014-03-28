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
                //action to be executed by each thread - concurrently mutate the deque
                Action action = () =>
                    {
                        int i = 0;
                        int poppedValue;

                        while (!cancelled)
                        {
                            //pop from/push onto the deque
                            i++;
                            if (i % 4 == 0)
                                deque.PushLeft(1);
                            else if (i % 3 == 0)
                                deque.PushRight(1);
                            else if (i % 2 == 0)
                                deque.TryPopLeft(out poppedValue);
                            else
                                deque.TryPopRight(out poppedValue);

                            //increment counters
                            countersContainer.Increment();
                        }

                        countersContainer.Complete();
                    };

                Action cancel = () =>
                    {   
                        cancelled = true;
                    };

                //launch a set of threads to mutate the deque concurrently
                action.RunInParallel(cancel, ThreadCount, RunningTime);

                countersContainer.PrintCounters();
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
    }
}
