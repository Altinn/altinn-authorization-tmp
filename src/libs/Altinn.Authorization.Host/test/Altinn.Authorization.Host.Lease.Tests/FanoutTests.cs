namespace Altinn.Authorization.Host.Lease.Tests;

public abstract class FanoutTests
{
    public abstract IAltinnLease Lease { get; set; }

    public async Task TestThreadAquireExplosion(int numThreads)
    {
        var threads = new List<Task>();
        for (int i = 0; i < numThreads; i++)
        {
            var loop = i;
            threads.Add(new(async () =>
            {
                var result = await Lease.TryAquireNonBlocking<LeaseContent>("test", CancellationToken.None);
                if (result.HasLease)
                {
                    var content = result.Data ?? new LeaseContent();
                    if (content.Data.TryGetValue(loop.ToString(), out var data))
                    {
                        content.Data[loop.ToString()] = data + 1;
                    }
                    else
                    {
                        content.Data[loop.ToString()] = 1;
                    }

                    await Lease.Put(result, content, default);
                }
            }));
        }

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            await thread;
        }

        var result = await Lease.TryAquireNonBlocking<LeaseContent>("test", default);

        Assert.True(result.Data.Data.Aggregate(0, (acc, entry) => acc + entry.Value) >= 1);
    }

    public class LeaseContent
    {
        public Dictionary<string, int> Data { get; set; } = new();
    }
}


