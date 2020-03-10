namespace MindMatrix.Aggregates
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Shouldly;

    public class DateTimeService : IDateTime
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    class Program
    {


        public static async Task Main(string[] args)
        {

            var threads = 24;
            var aggregates = 10000;
            var iterations = int.MaxValue;
            var isRunning = true;
            //var iterations = 20000;
            var settings = new AggregateSettings() { MaxMutationCommits = 100 };
            await using var context = new MongoDbContext<Counter>(settings, false, new DateTimeService(), "v2f66bb5f716d4743b2a8e80a568ac932");
            var aggregateIds = Enumerable.Range(0, aggregates).Select(x => $"agg_{x:X8}").ToArray();
            var totals = new int[aggregateIds.Length];
            var mutations = 0;
            var concurrency = 0;

            for (var i = 0; i < aggregateIds.Length; i++)
            {
                var aggregate = await context.Repository.GetLatest(aggregateIds[i]);
                totals[i] = aggregate.State.Count;
            }


            var sw = Stopwatch.StartNew();

            var tasks = Enumerable.Range(0, threads).Select(xx => Task.Run(async () =>
            {
                var m = 0;
                var it = 0;
                var r = new Random((xx * 1024 + 1024));
                var values = new int[aggregateIds.Length];

                while (isRunning)
                {
                    it++;
                    var idx = r.Next(0, aggregateIds.Length);
                    var c = r.Next(1, 5);
                    var increments = new int[c];
                    for (var p = 0; p < increments.Length; p++)
                    {
                        increments[p] = r.Next(2, 6);
                        values[idx] += increments[p];
                        m++;
                    }

                    while (true)
                    {

                        var aggregate = await context.Repository.GetLatest(aggregateIds[idx]);
                        var hash = aggregate.GetHashCode();
                        //Console.WriteLine($"[{xx:X1}] {i:X2} [{hash:X8}] :: [READ] {aggregate}");
                        var amount = 0;
                        var count = aggregate.State.Count;
                        var start = count;
                        //try
                        {

                            for (var p = 0; p < increments.Length; p++)
                            {
                                count += increments[p];
                                amount += increments[p];
                                aggregate.Apply(new Increment() { Amount = increments[p] });
                                aggregate.State.Count.ShouldBe(count);
                            }
                            var result = await aggregate.Commit();
                            aggregate = await context.Repository.GetLatest(aggregateIds[idx]);

                            // if (aggregate.State.Count != count)
                            //     Console.WriteLine($"[{xx:X1}] {i:X2} [{hash:X8}] :: *{result} Expected: {count}, Actual: {aggregate.State.Count}, {aggregate}");
                            // else
                            //Console.WriteLine($"[{xx:X1}] {i:X2} [{hash:X8}] :: {result} Amount: {amount}, {start} -> {aggregate.State.Count}, {aggregate}");
                            // try
                            // {
                            //     aggregate.State.Count.ShouldBe(count);
                            // }
                            // catch (Exception ex)
                            // {
                            //     Console.WriteLine($"{aggregateIds[idx]}: {ex.Message}");
                            //     throw;
                            // }
                            await Task.Yield();
                            //await Task.Delay(Math.Max(i * i * 50, 5000));
                            if (result.Status != CommitStatus.Concurrency)
                                break;

                            Interlocked.Increment(ref concurrency);
                        }
                        // catch (ConcurrencyException)
                        // {
                        //     Console.WriteLine($"[{xx:X1}] {i:X2} :: [Concurrency] Tried {start} -> {count} [{amount}], {aggregate}");
                        // }
                    }
                }

                for (var i = 0; i < aggregateIds.Length; i++)
                    Interlocked.Add(ref totals[i], values[i]);
                Interlocked.Add(ref iterations, it);
                Interlocked.Add(ref mutations, m);
            })).ToArray();

            while (!Console.KeyAvailable)
                await Task.Delay(10);

            isRunning = false;

            await Task.WhenAll(tasks);
            sw.Stop();

            for (var i = 0; i < aggregateIds.Length; i++)
            {
                var aggregate = await context.Repository.GetLatest(aggregateIds[i]);
                aggregate.State.Count.ShouldBe(totals[i]);
            }

            Console.WriteLine($"Elapsed: {sw.Elapsed}, Avg: {(float)sw.ElapsedMilliseconds / iterations}ms, {(int)(iterations / sw.Elapsed.TotalSeconds)}/s, Concurrency Errors: {concurrency}, Mutations: {mutations}");
        }
    }
}
