using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DequeNet.Tests.Perf
{
    /// <summary>
    /// Swallows every method call.
    /// Use this when you don't want to use perf counters and avoid their overhead.
    /// </summary>
    internal class NullCountersContainer : IPerfCountersContainer
    {
        public void Dispose()
        {
        }

        public void Increment()
        {
        }

        public void PrintCounters()
        {
        }
    }
}
