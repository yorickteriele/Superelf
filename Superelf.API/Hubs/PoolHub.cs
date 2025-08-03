using Microsoft.AspNetCore.SignalR;

namespace Superelf.API.Hubs;

public class PoolHub : Hub
{
    public async Task JoinPoolGroup(string poolId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, poolId);
    }
    
    public async Task LeavePoolGroup(string poolId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, poolId);
    }
}
