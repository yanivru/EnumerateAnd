using Microsoft.VisualStudio.TestPlatform.ObjectModel;

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
            IAsyncEnumerable<int> a = new OnlyOnce(AsyncEnumerable.Range(1, 10));
            var (count, sum) = await a.Query(x => x.CountAsync(), y => y.SumAsync());

            Assert.AreEqual(10, count);
            Assert.AreEqual(55, sum);
        }
    }

    class OnlyOnce : IAsyncEnumerable<int>
    {
        private readonly IAsyncEnumerable<int> _inner;
        private bool _isFirstTime = true;

        public OnlyOnce(IAsyncEnumerable<int> inner)
        {
            _inner = inner;
        }

        public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            if (!_isFirstTime)
            {
                throw new MultipleEnumerationException();
            }

            _isFirstTime = false;
            return _inner.GetAsyncEnumerator(cancellationToken);

        }
    }

    internal class MultipleEnumerationException : Exception
    {
    }
}