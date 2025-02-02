using AuthApi.Models;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var chatId = Context.GetHttpContext().Request.Query["chatId"];
        if (!string.IsNullOrEmpty(chatId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }
        await base.OnConnectedAsync();
        Console.WriteLine($"\n///\nКлиент добавлен в группу {chatId}///\n///");
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

    // Этот метод вызывается из клиента через SignalR
    public async Task SendMessageToUser(int userId, Messages message)
    {
        await Clients.User(userId.ToString()).SendAsync("ReceiveMessage", message);
        Console.WriteLine("////////////////////\nSendMessageToUser в ChatHub\n////////////////////");
    }
}