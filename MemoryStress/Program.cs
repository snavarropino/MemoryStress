using System.Collections.Concurrent;

ConcurrentBag<Leak> cache = [];

Console.WriteLine("Starting job!");

var mbToRequest = int.Parse(Environment.GetEnvironmentVariable("MB_TO_REQUEST")!);
var delay = int.Parse(Environment.GetEnvironmentVariable("SECONDS_DELAY")!);

while (true)
{
    Console.WriteLine("Requesting {0} MB",mbToRequest);
    AllocMemory(mbToRequest*1000, CancellationToken.None);
    await Task.Delay(TimeSpan.FromSeconds(delay));
}

return;

void AllocMemory(int kilobytes, CancellationToken cancellationToken)
{
    for (var i = 0; i < kilobytes; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var oneKb = new string('#', 512); // unicode: 2 bytes * 512 = 1Kb
        cache.Add(new Leak(oneKb));
    }
}

internal record Leak(string Buffer);