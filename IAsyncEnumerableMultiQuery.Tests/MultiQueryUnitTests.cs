using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;

namespace IAsyncEnumerableMultiQuery.Tests
{
    [TestClass]
    public class MultiQueryUnitTests
    {
        [TestMethod]
        public async Task Query_TwoQueries_TwoResults()
        {
            IAsyncEnumerable<int> a = AsyncEnumerable.Range(1, 10);

            var (count, sum) = await a.Query(x => x.CountAsync(), y => y.SumAsync());

            Assert.AreEqual(10, count);
            Assert.AreEqual(55, sum);
        }

        [TestMethod]
        public async Task Query_TwoQueries_OneEnumeration()
        {
            var a = AsyncEnumerable.Range(1, 10).ToTrackingEnumerator();
            var (count, sum) = await a.Query(x => x.CountAsync(), y => y.SumAsync());

            Assert.AreEqual(10, count);
            Assert.AreEqual(55, sum);
            Assert.AreEqual(10, a.History.Count);
        }

        [TestMethod]
        public async Task Query_TwoQueriesOneShortQuery_OneEnumeration()
        {
            var a = AsyncEnumerable.Range(1, 10).ToTrackingEnumerator();
            var (count, any) = await a.Query(x => x.CountAsync(), y => y.AnyAsync());

            Assert.AreEqual(10, count);
            Assert.AreEqual(10, a.History.Count);
            Assert.IsTrue(any);
        }

        [TestMethod]
        public async Task Query_TwoShortQueries_EnumerateOnlyWhatIsNeeded()
        {
            var a = AsyncEnumerable.Range(1, 10).ToTrackingEnumerator();
            var (any, any2) = await a.Query(x => x.AnyAsync(), y => y.AnyAsync());

            Assert.IsTrue(any);
            Assert.IsTrue(any2);
            Assert.AreEqual(1, a.History.Count);
        }
    }

    static class AsyncEnumerableBuilder
    {
        public static EnumerationHistory<T> ToTrackingEnumerator<T>(this IAsyncEnumerable<T> orignal)
        {
            return new EnumerationHistory<T>(orignal);
        }
    }

    class EnumerationHistory<T> : IAsyncEnumerable<T>
    {
        private readonly IAsyncEnumerable<T> _original;
        public List<T> History { get; } = new();

        public EnumerationHistory(IAsyncEnumerable<T> original)
        {
            _original = original;
        }

        public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            await foreach (var item in _original)
            {
                History.Add(item);
                yield return item;
            }
        }
    }

    internal class MultipleEnumerationException : Exception
    {
    }
}