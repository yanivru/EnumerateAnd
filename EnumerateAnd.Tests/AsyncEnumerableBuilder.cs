namespace EnumerateAnd.Tests
{
    static class AsyncEnumerableBuilder
    {
        public static EnumerationHistory<T> ToTrackingEnumerator<T>(this IAsyncEnumerable<T> orignal)
        {
            return new EnumerationHistory<T>(orignal);
        }
    }
}