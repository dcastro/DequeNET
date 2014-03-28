using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DequeNet.Tests.Perf
{
    /// <summary>
    /// Counts the number of operations performed per second, as well and the total number of operations performed.
    /// The counters are kept in the process's memory, and are updated using <see cref="Interlocked"/>.
    /// </summary>
    internal class SlimPerfCountersContainer : IPerfCountersContainer
    {
        private long _totalOperationsCount;
        private readonly Stopwatch _watch;

        /**
         * Eeach thread updates this thread-local counter. 
         * When a thread signals that it has finished incrementing its counter (by calling Complete()),
         * the thread's local count will be added to the total count (i.e., _totalOperationsCount).
         */
        [ThreadStatic] private static long _threadLocalOperationsCount;

        public SlimPerfCountersContainer()
        {
            _watch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (_watch.IsRunning)
                _watch.Stop();
        }

        /// <summary>
        /// Increment every counter in the container
        /// </summary>
        public void Increment()
        {
            //updates the thread-local counter
            _threadLocalOperationsCount++;
        }

        /// <summary>
        /// Signals the container that the current thread has finished incrementing the counters.
        /// Should be called exactly once by each thread incrementing the counters.
        /// </summary>
        public void Complete()
        {
            //conmit the thread-local counter
            //i.e., add the number of operations performed by this thread to _totalOperationsCounter
            Interlocked.Add(ref _totalOperationsCount, _threadLocalOperationsCount);
        }

        /// <summary>
        /// Print the counters values to the console.
        /// Should only be called after each thread incrementing the counters has called <see cref="IPerfCountersContainer.Complete"/>.
        /// </summary>
        public void PrintCounters()
        {
            long ms = _watch.ElapsedMilliseconds;
            long operationsCount = Volatile.Read(ref _totalOperationsCount);

            double opsPerSecond = ((double) operationsCount/ms*1000);

            Console.WriteLine("{0,-20} {1,-30}", "Total Operations", operationsCount.ToString("N0"));
            Console.WriteLine("{0,-20} {1,-30}", "Operations/sec", opsPerSecond.ToString("N0"));
        }
    }
}
