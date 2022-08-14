using Microsoft.AspNetCore.SignalR;
using SignalRRepro.Hubs;
using System.Diagnostics;

namespace SignalRRepro;

public class MessageSender : BackgroundService, IDisposable
{
    private readonly byte[] _dummyData = new byte[2048];
    private readonly HubConnectionManager<MyHub> _manager;

    public MessageSender(HubConnectionManager<MyHub> manager)
    {
        _manager = manager;
        Random.Shared.NextBytes(_dummyData);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(5));
        long count = 0;

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            var dt = DateTime.UtcNow;
            var message = new Message { SentTime = dt, time = dt.ToString("O"), id = count++, data = _dummyData };

            _manager.Broadcast(async (id, c, state) =>
            {
                var message = (Message)state;

                // using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var sw = Stopwatch.StartNew();
                var task = c.SendAsync("clock", message);
                bool delay = !task.IsCompletedSuccessfully;
                await task;
                sw.Stop();
                if (delay)
                {
                    // Print a message if there's back pressure
                    Console.WriteLine($"{id}: Backpressure! Took {sw.ElapsedMilliseconds}ms, id={message.id}, Behind={count - message.id} messages");
                }

            },
            message);
        }
    }

    private class Message
    {
        internal DateTime SentTime { get; set; }
        public string time { get; init; } = default!;
        public long id { get; init; }
        public byte[] data { get; init; } = default!;
    }
}