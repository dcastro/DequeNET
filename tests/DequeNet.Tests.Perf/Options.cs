using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace DequeNet.Tests.Perf
{
    internal class Options
    {
        [Option('s', "slim-counters", MutuallyExclusiveSet = "slimCounterType",
            HelpText = "Use lighter in-memory performance counters")]
        public bool UseSlimCounters { get; set; }

        [Option('p', "perf-counters", MutuallyExclusiveSet = "slimCounterType",
            HelpText = "Use Windows' performance counters, which can be observed on the Performance Monitor (perfmon.exe)")]
        public bool UsePerfCounters { get; set; }

        [Option('t', "threads", DefaultValue = 4,
            HelpText = "Number of threads to use to mutate the ConcurrentDeque<T>")]
        public int Threads { get; set; }

        [Option('d', "duration", DefaultValue = 10000,
            HelpText = "Duration of the test (in milliseconds)")]
        public int RunningTime { get; set; }
    }
}
