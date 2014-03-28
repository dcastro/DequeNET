using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DequeNet.Tests.Perf
{
    /// <summary>
    /// Counts the number of operations performed per second, as well and the total number of operations performed.
    /// Wraps around <see cref="PerformanceCounter"/>.
    /// </summary>
    internal class PerfCountersContainer : IPerfCountersContainer
    {
        private const string CountersCategory = "Concurrent Deque";

        //counters names
        private const string TotalOperationsName = "Total Operations";
        private const string OperationsPerSecondName = "Operations/sec";

        //counters and their initial samples
        private IDictionary<PerformanceCounter, CounterSample> _initialSamples; 

        public PerfCountersContainer()
        {
            CreateCounters();
            InitializeCounters();
        }

        private static void CreateCounters()
        {
            //delete existing counters
            if (PerformanceCounterCategory.Exists(CountersCategory))
                PerformanceCounterCategory.Delete(CountersCategory);

            //create new counters
            var counters = new CounterCreationDataCollection
                {
                    new CounterCreationData(TotalOperationsName,
                                            "Total number of random operations performed on the concurrent deque.",
                                            PerformanceCounterType.NumberOfItems64),
                    new CounterCreationData(OperationsPerSecondName,
                                            "Random operations performed on the concurrent deque per second",
                                            PerformanceCounterType.RateOfCountsPerSecond64)
                };
            PerformanceCounterCategory.Create(CountersCategory, "description",
                                              PerformanceCounterCategoryType.SingleInstance, counters);
        }

        private void InitializeCounters()
        {
            //create counters
            var totalOpsCounter = new PerformanceCounter(CountersCategory, TotalOperationsName, false);
            var operationsPerSecCounter = new PerformanceCounter(CountersCategory, OperationsPerSecondName, false);

            //collect initial samples
            _initialSamples = new Dictionary<PerformanceCounter, CounterSample>
                {
                    {totalOpsCounter, totalOpsCounter.NextSample()},
                    {operationsPerSecCounter, operationsPerSecCounter.NextSample()}
                };
        }

        public void Dispose()
        {
            foreach (var counter in _initialSamples.Keys)
                counter.Dispose();
        }

        public void Increment()
        {
            foreach (var counter in _initialSamples.Keys)
                counter.Increment();
        }

        public void PrintCounters()
        {
            foreach (var entry in _initialSamples)
            {
                var counter = entry.Key;
                var initialSample = entry.Value;
                var finalSample = counter.NextSample();

                var counterValue = CounterSample.Calculate(initialSample, finalSample);

                Console.WriteLine("{0,-20} {1,-30}", counter.CounterName, counterValue.ToString("N0"));
            }
        }
    }
}
