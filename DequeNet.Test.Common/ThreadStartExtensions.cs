using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Xunit;

namespace DequeNet.Test.Common
{
    public static class ThreadStartExtensions
    {
        private const int ThreadTimeout = 500;

        public static void RunInParallel(this ThreadStart action, Action cancel, int threadCount, int runningTime)
        {
            //start threads
            var threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(action);
                threads[i].Start();
            }

            //sleep
            Thread.Sleep(runningTime);

            //stop threads
            if (cancel != null)
                cancel();

            for (int i = 0; i < threadCount; i++)
            {
                Assert.True(threads[i].Join(ThreadTimeout));
            }
        }

        public static void RunInParallel(this ThreadStart action, int threadCount, int runningTime)
        {
            action.RunInParallel(null, threadCount, runningTime);
        }
    }
}
