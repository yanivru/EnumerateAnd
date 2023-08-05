namespace EnumerateAnd
{
    /// <summary>
    /// Run multiple queries in a single enumeration
    /// </summary>
    public static class EnumerateAndExtensions
    {
        /// <summary>
        /// Specify the first query to run and enables adding more queries after.
        /// </summary>
        /// <typeparam name="T">The type of the IASyncEnumerable</typeparam>
        /// <typeparam name="TQueryResult">The type of the result</typeparam>
        /// <param name="source">Source IAsyncEnumerable</param>
        /// <param name="query">The query to run</param>
        /// <returns>A tuple that contains information for further adding queries</returns>
        public static (IAsyncEnumerable<T>, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult>>) QueryAnd<T, TQueryResult>(this IAsyncEnumerable<T> source, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult>> query)
        {
            return (source, query);
        }

        /// <summary>
        /// Adds the second query
        /// </summary>
        public static (IAsyncEnumerable<T>, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult1>>, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult2>>) QueryAnd<T, TQueryResult1, TQueryResult2>(this (IAsyncEnumerable<T> source, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult1>> query1) prev, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult2>> query)
        {
            return (prev.source, prev.query1, query);
        }

        /// <summary>
        /// Adds the last query and runs both queries on the source IASyncEnumerable
        /// </summary>
        public static async ValueTask<(TQueryResult1, TQueryResult2)> QueryAsync<T, TQueryResult1, TQueryResult2>(this (IAsyncEnumerable<T> source, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult1>> query1) prev, Func<IAsyncEnumerable<T>, ValueTask<TQueryResult2>> query2)
        {
            var lazyEnumerable = new EnumerateAndRunner<T>(prev.source);

            var t = prev.query1(lazyEnumerable);
            var t2 = query2(lazyEnumerable);

            await lazyEnumerable.RunAsync();

            return (await t, await t2);
        }

        /// <summary>
        /// Adds the last query and runs all three queries on the source IASyncEnumerable
        /// </summary>
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