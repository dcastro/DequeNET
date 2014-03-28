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
        private long _operationsCount;
        private readonly Stopwatch _watch;

        public SlimPerfCountersContainer()
        {
            _watch = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            if (_watch.IsRunning)
                _watch.Stop();
        }

        public void Increment()
        {
            Interlocked.Increment(ref _operationsCount);
        }

        public void PrintCounters()
        {
            long ms = _watch.ElapsedMilliseconds;
            long operationsCount = Volatile.Read(ref _operationsCount);

            double opsPerSecond = ((double) operationsCount/ms*1000);

            Console.WriteLine("{0,-20} {1,-30}", "Total Operations", operationsCount.ToString("N0"));
            Console.WriteLine("{0,-20} {1,-30}", "Operations/sec", opsPerSecond.ToString("N0"));
        }
    }
}
