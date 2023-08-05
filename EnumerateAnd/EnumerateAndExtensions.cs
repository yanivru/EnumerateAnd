namespace EnumerateAnd
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
}