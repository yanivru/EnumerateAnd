namespace EnumerateAnd
{
    internal class OneToManyEnumerator<T> : IAsyncEnumerator<T>
    {
        private TaskCompletionSource<bool> _isCurrentItemReady = new();
        private TaskCompletionSource<bool> _isReadyToReceiveMore = new();
        private T? _current;

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

        public async ValueTask<bool> IsReadyToReceiveMoreAsync()
        {
            var isStillActive = await _isReadyToReceiveMore.Task;
            _isReadyToReceiveMore = new TaskCompletionSource<bool>();
            return isStillActive;
        }

        internal void SetNoMoreResults()
        {
            _isCurrentItemReady.TrySetResult(false);
        }

        public T Current
        {
            get => _current ?? throw new Exception("Current is not initialized. Did you forget to call MoveNextAsync?");
            private set => _current = value;
        }
    }
}