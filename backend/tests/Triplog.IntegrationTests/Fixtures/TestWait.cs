namespace Triplog.IntegrationTests.Fixtures;

public static class TestWait
{
    public static async Task<T> ForAsync<T>(
        Func<Task<T>> fetch,
        Func<T, bool> predicate,
        TimeSpan timeout,
        TimeSpan? pollInterval = null)
    {
        pollInterval ??= TimeSpan.FromMilliseconds(200);
        var deadline = DateTime.UtcNow + timeout;
        T last = default!;

        while (DateTime.UtcNow < deadline)
        {
            last = await fetch();
            if (predicate(last)) return last;
            await Task.Delay(pollInterval.Value);
        }

        throw new TimeoutException(
            $"Predicate not satistied within {timeout.TotalSeconds}s. Last obsered: {last}");
    }
}
