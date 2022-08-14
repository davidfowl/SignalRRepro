using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace SignalRRepro;

public class HubConnectionManager<T> where T : Hub
{
    private readonly IHubContext<T> _hubContext;
    private readonly ConcurrentDictionary<string, Channel<(Func<string, ISingleClientProxy, object, Task>, object)>> _store = new();

    public HubConnectionManager(IHubContext<T> hubContext)
    {
        _hubContext = hubContext;
    }

    public void AddConnection(string connectionId)
    {
        var channel = Channel.CreateBounded<(Func<string, ISingleClientProxy, object, Task>, object)>(new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        });

        if (_store.TryAdd(connectionId, channel))
        {
            var client = _hubContext.Clients.Client(connectionId);

            _ = Task.Run(async () =>
            {
                await foreach (var (cb, state) in channel.Reader.ReadAllAsync())
                {
                    await cb(connectionId, client, state);
                }
            });
        }
    }

    public void Broadcast(Func<string, ISingleClientProxy, object, Task> callback, object state)
    {
        foreach (var (_, channel) in _store)
        {
            channel.Writer.TryWrite((callback, state));
        }
    }

    public void RemoveConnection(string connectionId)
    {
        if (_store.TryRemove(connectionId, out var channel))
        {
            channel.Writer.TryComplete();
        }
    }
}
