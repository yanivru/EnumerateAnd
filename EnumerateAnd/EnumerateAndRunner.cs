
namespace EnumerateAnd
{
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

            //await foreach (var item in _source)
            await using (var enumerator = _source.GetAsyncEnumerator())
            {
                while (true)
                {
                    isMoreNeeded1 = isMoreNeeded1 && await _oneToManyEnumerator1.IsReadyToReceiveMoreAsync();
                    isMoreNeeded2 = isMoreNeeded2 && await _oneToManyEnumerator2.IsReadyToReceiveMoreAsync();
                    isMoreNeeded3 = isMoreNeeded3 && _enumeratorIndex == 3 && await _oneToManyEnumerator3.IsReadyToReceiveMoreAsync();

                    if (!isMoreNeeded1 && !isMoreNeeded2 && !isMoreNeeded3)
                        break;

                    if (!await enumerator.MoveNextAsync())
                        break;

                    var item = enumerator.Current;

                    if (isMoreNeeded1)
                        _oneToManyEnumerator1.SetResult(item);
                    if (isMoreNeeded2)
                        _oneToManyEnumerator2.SetResult(item);
                    if (isMoreNeeded3)
                        _oneToManyEnumerator3.SetResult(item);
                }
            }
            _oneToManyEnumerator1.SetNoMoreResults();
            _oneToManyEnumerator2.SetNoMoreResults();
            _oneToManyEnumerator3.SetNoMoreResults();
        }
    }
}