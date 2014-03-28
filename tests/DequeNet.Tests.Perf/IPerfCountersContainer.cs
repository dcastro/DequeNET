using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DequeNet.Tests.Perf
{
    internal interface IPerfCountersContainer : IDisposable
    {
        void Increment();

        void PrintCounters();
    }
}
