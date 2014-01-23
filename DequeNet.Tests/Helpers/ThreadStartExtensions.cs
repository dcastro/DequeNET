using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Xunit;

namespace DequeNet.Tests.Helpers
{
    internal static class ThreadStartExtensions
    {
        private const int ThreadTimeout = 500;

        /// <summary>
        /// Executes a given action in a given number of threads.
        /// These threads are stopped after the specified <paramref name="runningTime"/> has passed.
        /// </summary>
        /// <param name="action">The action that will be called when the threads start.</param>
        /// <param name="cancel">The action that will be called to cancel the threads.</param>
        /// <param name="threadCount">The number of threads to spawn.</param>
        /// <param name="runningTime">The time to let the threads run (ms).</param>
        public static void RunInParallel(this ThreadStart action, Action cancel, int threadCount, int runningTime)
        {
            Exception exThrown = null;

            //encapsulate threadstart
            ThreadStart threadStart = () =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        exThrown = ex;
                    }
                };


            //start threads
            var threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(threadStart);
                thread.Start();
                threads[i] = thread;
            }

            //sleep
            if(runningTime >= 0)
                Thread.Sleep(runningTime);

            //stop threads
            if(cancel != null)
                cancel();

            for (int i = 0; i < threadCount; i++)
            {
                Assert.True(threads[i].Join(ThreadTimeout));
            }

            //assert exceptions weren't thrown
            //if one was thrown, rethrow it while preserving its stacktrace
            if (exThrown != null)
                ExceptionDispatchInfo.Capture(exThrown).Throw();
        }

        /// <summary>
        /// Executes a given action in a given number of threads indefinitely.
        /// </summary>
        /// <param name="action">The action that will be called when the threads start.</param>
        /// <param name="threadCount">The number of threads to spawn.</param>
        public static void RunInParallel(this ThreadStart action, int threadCount)
        {
            action.RunInParallel(null, threadCount, -1);
        }

        /// <summary>
        /// Starts a given number of threads and returns them.
        /// </summary>
        /// <param name="action">The action that will be called when the threads start.</param>
        /// <param name="threadCount">The number of threads to spawn.</param>
        /// <returns>The created set of threads.</returns>
        public static Thread[] StartInParallel(this ThreadStart action, int threadCount)
        {
            //start threads
            var threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(action);
                thread.Start();
                threads[i] = thread;
            }

            return threads;
        }
    }
}
