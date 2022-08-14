using Microsoft.AspNetCore.SignalR;

namespace SignalRRepro.Hubs;

public class MyHub : Hub
{
    private readonly HubConnectionManager<MyHub> _manager;

    public MyHub(HubConnectionManager<MyHub> store)
    {
        _manager = store;
    }

    public override Task OnConnectedAsync()
    {
        _manager.AddConnection(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _manager.RemoveConnection(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}