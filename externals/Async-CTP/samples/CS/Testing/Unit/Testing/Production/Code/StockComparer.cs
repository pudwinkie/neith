using System;
using System.Linq;
using System.Threading.Tasks;

public static class StockComparer
{
    public static async Task<SnapshotExtrema> ComparePercentsAsync(IStockSnapshotProvider[] stockProviders)
    {
        if (stockProviders.Length < 1)
        {
            throw new ArgumentException("Must provide at least one provider.", "stockProviders");
        }

        // initialize our extrema so that we don't have to special-case the first element
        var extrema = new SnapshotExtrema
        {
            MinDelta = { Percent = Double.PositiveInfinity },
            MaxDelta = { Percent = Double.NegativeInfinity }
        };

        // LINQ to objects is an easy way to synchronously call GetLatestSnapshotAsync() on all of them and aggregate the tasks
        var providerTasks = (from provider in stockProviders
                             select provider.GetLatestSnapshotAsync()).ToList(); // use ToList() to force eval of the lazy LINQ enumerable

        foreach (var providerTask in providerTasks)
        {
            // await the next provider task
            await providerTask;

            var snapshot = providerTask.Result;

            if (extrema.MaxDelta.Percent < snapshot.Percent)
            {
                // new high for max delta
                extrema.MaxDelta = snapshot;
            }

            if (snapshot.Percent < extrema.MinDelta.Percent)
            {
                // new low for min delta
                extrema.MinDelta = snapshot;
            }
        }

        return extrema;
    }
}