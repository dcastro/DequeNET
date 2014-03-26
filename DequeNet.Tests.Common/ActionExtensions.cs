using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DequeNet.Tests.Common
{
    public static class ActionExtensions
    {
        private const int ThreadTimeout = 1000;

        /// <summary>
        /// Executes a given action in a given number of threads.
        /// These threads are stopped after the specified <paramref name="runningTime"/> has passed.
        /// </summary>
        /// <param name="action">The action that will be called when the threads start.</param>
        /// <param name="cancel">The action that will be called to cancel the threads.</param>
        /// <param name="threadCount">The number of threads to spawn.</param>
        /// <param name="runningTime">The time to let the threads run (ms).</param>
        public static void RunInParallel(this Action action, Action cancel, int threadCount, int runningTime)
        {
            var exceptionsThrown = new ConcurrentBag<Exception>();

            //encapsulate threadstart
            ThreadStart threadStart = () =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exceptionsThrown.Add(ex);
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
            if (runningTime >= 0)
                Thread.Sleep(runningTime);

            //stop threads
            if (cancel != null)
                cancel();

            //join threads
            for (int i = 0; i < threadCount; i++)
            {
                //throw if a thread fails to join within the given timeout
                if(!threads[i].Join(ThreadTimeout))
                    throw new TimeoutException(
                        string.Format("Thread #{0} failed to complete within {1} milliseconds.", i, ThreadTimeout));
            }

            //if any exceptions were thrown, group them and rethrow the aggregate
            if (!exceptionsThrown.IsEmpty)
                throw new AggregateException(exceptionsThrown);
        }

        /// <summary>
        /// Executes a given action in a given number of threads indefinitely.
        /// </summary>
        /// <param name="action">The action that will be called when the threads start.</param>
        /// <param name="threadCount">The number of threads to spawn.</param>
        public static void RunInParallel(this Action action, int threadCount)
        {
            action.RunInParallel(null, threadCount, -1);
        }

        /// <summary>
        /// Starts a given number of threads and returns them.
        /// </summary>
        /// <param name="action">The action that will be called when the threads start.</param>
        /// <param name="threadCount">The number of threads to spawn.</param>
        /// <returns>The created set of threads.</returns>
        public static Thread[] StartInParallel(this Action action, int threadCount)
        {
            //start threads
            var threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var thread = new Thread(() => action());
                thread.Start();
                threads[i] = thread;
            }

            return threads;
        }
    }
}
