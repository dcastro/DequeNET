using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace DequeNet.Tests.Helpers
{
    /// <summary>
    /// Marks a test method to be executed a given number of times.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RepeatTestAttribute : FactAttribute
    {
        readonly int _count;

        /// <summary>
        /// Marks a test method to be executed a given number of times.
        /// </summary>
        /// <param name="count">The number of times to repeat the test.</param>
        public RepeatTestAttribute(int count)
        {
            _count = count;
        }

        /// <summary>
        /// Creates instances of <see cref="TheoryCommand"/> which represent individual intended
        /// invocations of the test method.
        /// </summary>
        /// <param name="method">The method under test</param>
        /// <returns>An enumerator through the desired test method invocations</returns>
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            var results = new List<ITestCommand>();

            for (int i = 0; i < _count; i++)
                results.Add(new TheoryCommand(method, new object[] { }, null));

            return results;
        }
    }
}
