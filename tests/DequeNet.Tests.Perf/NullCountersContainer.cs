using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DequeNet.Tests.Perf
{
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
