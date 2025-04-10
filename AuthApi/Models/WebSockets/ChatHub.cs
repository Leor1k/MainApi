using AuthApi.Models;
using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        
        var userId = Context.GetHttpContext().Request.Query["userId"];
        if (!string.IsNullOrEmpty(userId))
        {
            
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"Клиент {Context.ConnectionId} добавлен в группу {userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Извлекаем UserId из query string
        var userId = Context.GetHttpContext().Request.Query["userId"];
        if (!string.IsNullOrEmpty(userId))
        {
            // Удаляем клиента из группы
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"Клиент {Context.ConnectionId} удалён из группы {userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessageToUser(string receiverUserId, Messages message)
    {
        var groupClients = Clients.Group(receiverUserId);
        if (groupClients != null)
        {
            await groupClients.SendAsync("ReceiveMessage", message);
            Console.WriteLine($"Сообщение отправлено пользователю {receiverUserId}");
        }
        else
        {
            Console.WriteLine($"Нет подключений для пользователя {receiverUserId}");
        }

    }
}