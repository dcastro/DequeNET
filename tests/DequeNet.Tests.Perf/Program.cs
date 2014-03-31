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
        private static int _runningTime;
        private static int _threadCount;

        public static void Main(string[] args)
        {
            var options = new Options();
            var deque = new ConcurrentDeque<int>(Enumerable.Repeat(1, 100000));
            bool cancelled = false;

            //parse arguments
            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
                return;

            Setup(options);

            using (var countersContainer = CreateContainer(options))
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
                action.RunInParallel(cancel, _threadCount, _runningTime);

                countersContainer.PrintCounters();
            }
        }

        private static IPerfCountersContainer CreateContainer(Options options)
        {
            if (options.UseSlimCounters)
                return new SlimPerfCountersContainer();

            if (options.UsePerfCounters)
                return new PerfCountersContainer();

            return new NullCountersContainer();
        }

        private static void Setup(Options options)
        {
            _runningTime = options.RunningTime;
            _threadCount = options.Threads;
        }
    }
}
