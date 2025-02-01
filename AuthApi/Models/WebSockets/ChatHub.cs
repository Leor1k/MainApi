using AuthApi.Models;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
        Console.WriteLine("////////////////////\nOnConnectedAsync\n////////////////////");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnDisconnectedAsync(exception);
    }
    public async Task SendMessage(string userId, Messages message)
    {
        await Clients.User(userId).SendAsync("ReceiveMessage", message);
        Console.WriteLine("////////////////////\nSendMessage в ChutHub\n////////////////////");
    }
}
