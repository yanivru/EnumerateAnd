namespace EnumerateAnd.Tests
{
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
}