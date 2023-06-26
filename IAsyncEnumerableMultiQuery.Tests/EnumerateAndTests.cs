using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IAsyncEnumerableMultiQuery.Tests
{
    [TestClass]
    public class EnumerateAndTests
    {
        [TestMethod]
        public async Task QueryAnd_Query_ReturnsTheQueryValueAsync()
        {
            var sourceEnumerable = AsyncEnumerable.Range(1, 10).ToTrackingEnumerator();

            (_, var count) = await sourceEnumerable.QueryAnd(x => DoSomethingAsync(x))
                .QueryAsync(x => x.CountAsync());

            Assert.AreEqual(10, count);
            Assert.AreEqual(10, sourceEnumerable.History.Count);
        }

        [TestMethod]
        public async Task QueryAnd_3Queries_ReturnsTheQueryValueAsync()
        {
            var sourceEnumerable = AsyncEnumerable.Range(1, 10).ToTrackingEnumerator();

            (_, var any, var count) = await sourceEnumerable.QueryAnd(x => DoSomethingAsync(x))
                .QueryAnd(x => x.AnyAsync())
                .QueryAsync(x => x.CountAsync());

            Assert.IsTrue(any);
            Assert.AreEqual(10, count);
            Assert.AreEqual(10, sourceEnumerable.History.Count);
        }

        private static async ValueTask<bool> DoSomethingAsync(IAsyncEnumerable<int> x)
        {
            await foreach(var item in x)
            {
                if (item > 10_000)
                    throw new Exception();
            }

            return true;
        }
    }
}
