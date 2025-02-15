using System.Collections.Concurrent;
using AuthApi.Models.VoiceModels;
using Microsoft.AspNetCore.SignalR;

namespace AuthApi.Models.WebSockets
{
    public class VoiceHub : Hub
    {
        private static ConcurrentDictionary<string, VoiceCallSession> _activeCalls = new();
        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"Пользователь {userId} подключился к VoiceHub");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.GetHttpContext().Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                Console.WriteLine($"Пользователь {userId} отключился от VoiceHub");
            }
            await base.OnDisconnectedAsync(exception);
        }

        // 1. Запрос на начало звонка
        public async Task StartCall(string callerId, List<string> participantIds)
        {
            string roomId = Guid.NewGuid().ToString();
            var callSession = new VoiceCallSession(roomId, callerId, participantIds);
            _activeCalls[roomId] = callSession;

            Console.WriteLine($"Пользователь {callerId} начал звонок в комнате {roomId}");

            foreach (var participantId in participantIds.Where(id => id != callerId))
            {
                await Clients.Group(participantId).SendAsync("IncomingCall", new
                {
                    RoomId = roomId,
                    CallerId = callerId
                });
            }

        }

        // 2. Подтверждение звонка
        public async Task AcceptCall(string userId, string roomId)
        {
            if (_activeCalls.TryGetValue(roomId, out var callSession))
            {
                callSession.AcceptCall(userId);

                Console.WriteLine($"Пользователь {userId} принял звонок в комнате {roomId}");

                // Если хотя бы один принял – отправляем данные в голосовой сервер
                if (callSession.HasAcceptedUsers())
                {
                    await Clients.Group(callSession.CallerId).SendAsync("CallAccepted", new
                    {
                        RoomId = roomId,
                        Participants = callSession.GetAcceptedUsers()
                    });

                    Console.WriteLine($"Начинаем голосовой вызов в комнате {roomId}");
                }
            }
        }

        // 3. Отклонение звонка
        public async Task RejectCall(string userId, string roomId)
        {
            if (_activeCalls.TryGetValue(roomId, out var callSession))
            {
                callSession.RejectCall(userId);
                Console.WriteLine($"Пользователь {userId} отклонил звонок в комнате {roomId}");
            }
        }
    }
}
