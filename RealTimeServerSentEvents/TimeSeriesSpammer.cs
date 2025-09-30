using System.Runtime.CompilerServices;
using Modity.TimeSeries;

namespace RealTimeServerSentEvents;

public class TimeSeriesSpammer
{
    public async IAsyncEnumerable<TimeSeriesEvent> GetTimeSeriesUpdates(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var list = Enumerable.Range(0, 5)
                .Select(_ => new TimeSeries<decimal>(
                    Enumerable.Range(0, 10)
                        .Select(__ => new TimeSeriesValue<decimal>(DateTimeOffset.Now, 5 + 1))
                        .ToList()))
                .ToList();

            yield return new TimeSeriesEvent(Guid.NewGuid().ToString("N"), list);
            
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
    }

    public IAsyncEnumerable<TimeSeriesEvent> GenerateTimeSeriesSince(string? lastEventId, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(lastEventId))
        {
            Console.WriteLine("lets pretend this matter...");
        }
        return GetTimeSeriesUpdates(ct);
    }
}