using Microsoft.VisualStudio.Threading;

namespace IAsyncEnumerableMultiQuery
{
    public static class EnumerateAndExtensions
    {
        public static (IAsyncEnumerable<T>, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult>>) QueryAnd<T, TQueryResult>(this IAsyncEnumerable<T> source, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult>> query)
        {
            return (source, query);
        }

        public static (IAsyncEnumerable<T>, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult1>>, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult2>>) QueryAnd<T, TQueryResult1, TQueryResult2>(this (IAsyncEnumerable<T> source, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult1>> query1) prev, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult2>> query)
        {
            return (prev.source, prev.query1, query);
        }

        public static async ValueTask<(TQueryResult1, TQueryResult2)> QueryAsync<T, TQueryResult1, TQueryResult2>(this (IAsyncEnumerable<T> source, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult1>> query1) prev, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult2>> query2)
        {
            var lazyEnumerable = new EnumerateAndRunner<T>(prev.source);

            var t = prev.query1(lazyEnumerable);
            var t2 = query2(lazyEnumerable);

            await lazyEnumerable.RunAsync();

            return (await t, await t2);
        }

        public static async ValueTask<(TQueryResult1, TQueryResult2, TQueryResult3)> QueryAsync<T, TQueryResult1, TQueryResult2, TQueryResult3>(this (IAsyncEnumerable<T> source, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult1>> query1, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult2>> query2) prev, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult3>> query3)
        {
            var lazyEnumerable = new EnumerateAndRunner<T>(prev.source);

            var t = prev.query1(lazyEnumerable);
            var t2 = prev.query2(lazyEnumerable);
            var t3 = query3(lazyEnumerable);

            await lazyEnumerable.RunAsync();

            return (await t, await t2, await t3);
        }
    }

    internal class EnumerateAndRunner<T> : IAsyncEnumerable<T>
    {
        private readonly IAsyncEnumerable<T> _source;
        private readonly OneToManyEnumerator<T> _oneToManyEnumerator1;
        private readonly OneToManyEnumerator<T> _oneToManyEnumerator2;
        private readonly OneToManyEnumerator<T> _oneToManyEnumerator3;
        private int _enumeratorIndex;

        public EnumerateAndRunner(IAsyncEnumerable<T> source)
        {
            _source = source;
            _oneToManyEnumerator1 = new OneToManyEnumerator<T>();
            _oneToManyEnumerator2 = new OneToManyEnumerator<T>();
            _oneToManyEnumerator3 = new OneToManyEnumerator<T>();
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            _enumeratorIndex++;

            return _enumeratorIndex switch
            {
                1 => _oneToManyEnumerator1,
                2 => _oneToManyEnumerator2,
                3 => _oneToManyEnumerator3,
                _ => throw new Exception("EnumerateAnd only supports up to 3 querys"),
            };
        }

        public async Task RunAsync()
        {
            bool isMoreNeeded1 = true;
            bool isMoreNeeded2 = true;
            bool isMoreNeeded3 = true;

            await foreach (var item in _source)
            {
                isMoreNeeded1 = isMoreNeeded1 && await _oneToManyEnumerator1.IsReadyToRecieveMoreAsync();
                isMoreNeeded2 = isMoreNeeded2 && await _oneToManyEnumerator2.IsReadyToRecieveMoreAsync();
                isMoreNeeded3 = isMoreNeeded3 && _enumeratorIndex == 3 && await _oneToManyEnumerator3.IsReadyToRecieveMoreAsync();

                if (isMoreNeeded1)
                    _oneToManyEnumerator1.SetResult(item);
                if (isMoreNeeded2)
                    _oneToManyEnumerator2.SetResult(item);
                if (isMoreNeeded3)
                    _oneToManyEnumerator3.SetResult(item);

                if (!isMoreNeeded1 && !isMoreNeeded2 && !isMoreNeeded3)
                    break;
            }

            _oneToManyEnumerator1.SetNoMoreResults();
            _oneToManyEnumerator2.SetNoMoreResults();
            _oneToManyEnumerator3.SetNoMoreResults();
        }
    }

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

    class OneToManyEnumerator<T> : IAsyncEnumerator<T>
    {
        private TaskCompletionSource<bool> _isCurrentItemReady = new();
        private TaskCompletionSource<bool> _isReadyToReceiveMore = new();

        public void SetResult(T result)
        {
            Current = result;
            var prevRead = _isCurrentItemReady;
            _isCurrentItemReady = new TaskCompletionSource<bool>();
            prevRead.SetResult(true);
        }

        public ValueTask DisposeAsync()
        {
            _isReadyToReceiveMore.TrySetResult(false);
            return ValueTask.CompletedTask;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            _isReadyToReceiveMore.SetResult(true);
            return await _isCurrentItemReady.Task;
        }

        public async ValueTask<bool> IsReadyToRecieveMoreAsync()
        {
            var isStillActive = await _isReadyToReceiveMore.Task;
            _isReadyToReceiveMore = new TaskCompletionSource<bool>();
            return isStillActive;
        }

        internal void SetNoMoreResults()
        {
            _isCurrentItemReady.TrySetResult(false);
        }

        public T Current { get; private set; }
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

            if (newQueryCount == 0)
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