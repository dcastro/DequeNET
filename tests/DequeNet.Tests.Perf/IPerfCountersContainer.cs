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
        /// <summary>
        /// Increment every counter in the container
        /// </summary>
        void Increment();

        /// <summary>
        /// Signals the container that the current thread has finished incrementing the counters.
        /// Should be called exactly once by each thread incrementing the counters.
        /// </summary>
        void Complete();

        /// <summary>
        /// Print the counters values to the console.
        /// Should only be called after each thread incrementing the counters has called <see cref="Complete"/>.
        /// </summary>
        void PrintCounters();
    }
}
