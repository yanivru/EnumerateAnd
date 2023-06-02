namespace IAsyncEnumerableMultiQuery
{
    public static class MultiQuery
    {
        public static async Task<(T1, T2)> Query<T, T1, T2>(this IAsyncEnumerable<T> source, Func<IAsyncEnumerable<T>, ValueTask<T1>> query1, Func<IAsyncEnumerable<T>, ValueTask<T2>> query2)
        {
            var e = new SingleEnumerable<T>(source, 2);
            var r1 = query1(e);
            var r2 = query2(e);
            return (await r1, await r2);
        }
    }

    class SingleEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly SingleEnumerator<T> _singleEnumerator;

        public SingleEnumerable(IAsyncEnumerable<T> inner, int queryCount)
        {
            _singleEnumerator = new SingleEnumerator<T>(inner, queryCount);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
        {
            return _singleEnumerator;
        }
    }

    class SingleEnumerator<T> : IAsyncEnumerator<T>
    {
        private int _queryCount;
        private int _waitingCount;
        private int _disposedCount = 0;
        private TaskCompletionSource<bool> _ready = new TaskCompletionSource<bool>();
        private IAsyncEnumerator<T> _innerEnumerator;

        public SingleEnumerator(IAsyncEnumerable<T> inner, int queryCount)
        {
            _queryCount = queryCount;
            _innerEnumerator = inner.GetAsyncEnumerator();
        }

        public ValueTask DisposeAsync()
        {
            var newQueryCount = Interlocked.Decrement(ref _queryCount);

            if(newQueryCount == 0)
            {
                _innerEnumerator.DisposeAsync();
                return ValueTask.CompletedTask;
            }

            if (newQueryCount == _waitingCount)
            {
                _ = MoveToNextElement();
            }

            return ValueTask.CompletedTask;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (Interlocked.Increment(ref _waitingCount) == _queryCount)
            {
                return await MoveToNextElement();
            }
            return await _ready.Task;
        }

        private async ValueTask<bool> MoveToNextElement()
        {
            Interlocked.Exchange(ref _waitingCount, 0);
            var ready = _ready;
            _ready = new TaskCompletionSource<bool>();
            ready.SetResult(await _innerEnumerator.MoveNextAsync());
            return await ready.Task;
        }

        public T Current => _innerEnumerator.Current;
    }
}