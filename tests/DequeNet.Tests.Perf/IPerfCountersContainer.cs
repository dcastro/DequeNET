using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DequeNet.Tests.Perf
{
    /// <summary>
    /// Counts the number of operations performed per second, as well and the total number of operations performed.
    /// </summary>
    internal interface IPerfCountersContainer : IDisposable
    {
        void Increment();

        void PrintCounters();
    }
}
