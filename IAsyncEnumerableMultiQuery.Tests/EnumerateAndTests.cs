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
        public async Task QueryAnd_Enumerate_ReturnsTheQueryValueAsync()
        {
            var sourceEnumerable = AsyncEnumerable.Range(1, 10).ToTrackingEnumerator();

            await sourceEnumerable.EnumerateAnd(x => DoSomething(x))
                .QueryAsync(x => x.CountAsync());
        }

        private async ValueTask DoSomething(IAsyncEnumerable<int> x)
        {
            await foreach(var item in x)
            {
                if (item > 10_000)
                    throw new Exception();
            }
        }
    }
}
