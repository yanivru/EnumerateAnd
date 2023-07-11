# EnumerateAnd

Performs multiple queries while enumerating the IAsyncEnumerable only once.

# Usage
In the following example the sourceEnumerable is enumerated only once even though we execute both AnyAsync and CountAsync, both from System.Linq.Async package.
            ```csharp
            var sourceEnumerable = AsyncEnumerable.Range(1, 10);

            (var any, var count) = await sourceEnumerable
                .QueryAnd(x => x.AnyAsync())
                .QueryAsync(x => x.CountAsync());

            Assert.IsTrue(any);
            Assert.AreEqual(10, count);
            ```
Any method that accepts IAsyncEnumerable as a parameter can be used:
            ```csharp
            var sourceEnumerable = AsyncEnumerable.Range(1, 10);

            (_, var count) = await sourceEnumerable.QueryAnd(x => DoSomethingAsync(x))
                .QueryAsync(x => x.CountAsync());

            Assert.AreEqual(10, count);
            Assert.AreEqual(10, sourceEnumerable.History.Count);

            private static async ValueTask<bool> DoSomethingAsync(IAsyncEnumerable<int> x)
            {
                await foreach(var item in x)
                {
                  Console.WriteLine(item);
                }
  
                // It must return something (There is now support for async Actions for now).
                return true;
            }
            ```
            
Up to 3 queries can be run on the IAsyncEnumerable.
