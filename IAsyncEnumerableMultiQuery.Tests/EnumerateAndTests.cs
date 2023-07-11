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

        [TestMethod]
        public async Task QueryAnd_ShortQueries_StopsEnumeratingSourceAsync()
        {
            var sourceEnumerable = AsyncEnumerable.Range(1, 10).ToTrackingEnumerator();

            (var any, var count) = await sourceEnumerable
                .QueryAnd(x => x.AnyAsync())
                .QueryAsync(x => x.FirstAsync());

            Assert.IsTrue(any);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, sourceEnumerable.History.Count);
        }

        [TestMethod]
        public async Task QueryAnd_ExceptionIsThrown_ExceptionThrownAsync()
        {
            var sourceEnumerable = AsyncEnumerable.Range(1, 10)
                .Select(x => x == 1? throw new Exception("Something went wrong") : x)
                .ToTrackingEnumerator();

            await Assert.ThrowsExceptionAsync<Exception>(async () => await sourceEnumerable
                .QueryAnd(x => x.AnyAsync())
                .QueryAsync(x => x.FirstAsync()));
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
