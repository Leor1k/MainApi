using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using System;
using AuthApi.Models.VoiceModels;

namespace AuthApi.Models.WebSockets
{
    public class VoiceHub : Hub
    {
        private static ConcurrentDictionary<string, VoiceCallSession> _activeCalls = new();
        private readonly VoiceService _voiceService;

        public VoiceHub(VoiceService voiceService)
        {
            _voiceService = voiceService;
        }

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

        public async Task StartCall(string roomId, string callerId, List<string> participantIds)
        {
            Console.WriteLine($"[WebStartCall] Прилетел запрос от {callerId} на создание комнаты");
            var callSession = new VoiceCallSession(roomId, callerId, participantIds);
            _activeCalls[roomId] = callSession;

            Console.WriteLine($"[WebStartCall] Пользователь {callerId} начал звонок в комнате {roomId}");

            bool success = await _voiceService.StartCallAsync(roomId, callerId, participantIds);
            if (!success)
            {
                Console.WriteLine($"[WebStartCall] Ошибка отправки запроса в VoiceModul для комнаты {roomId}");
                await Clients.Caller.SendAsync("Error", "Ошибка соединения с VoiceModul");
                return;
            }

            foreach (var participantId in participantIds.Where(id => id != callerId))
            {
                await Clients.Group(participantId).SendAsync("IncomingCall", roomId, callerId);
                Console.WriteLine($"[WebStartCall] С комнаты {roomId} оправляется звонок юзеру с id {participantId} от {callerId}");
            }

            Console.WriteLine($"[WebStartCall] Создана комната {roomId}, активные комнаты: {string.Join(", ", _activeCalls.Keys)}");
        }

        public async Task AcceptCall(string userId, string roomId)
        {
            if (!_activeCalls.TryGetValue(roomId, out var callSession))
            {
                Console.WriteLine("[WebAcceptCall] Не найден звонок с таким RoomId!");
                await Clients.Caller.SendAsync("Error", "Не найден звонок с таким RoomId!");
                return;
            }

            bool success = await _voiceService.ConfirmCallAsync(roomId, userId);
            if (!success)
            {
                Console.WriteLine($"[WebAcceptCall] Ошибка подтверждения звонка в VoiceModul для {userId}");
                await Clients.Caller.SendAsync("Error", "Ошибка подключения к голосовому серверу");
                return;
            }

            callSession.AcceptCall(userId);
            Console.WriteLine($"Пользователь {userId} принял звонок в комнате {roomId}");
        }

        public async Task RejectCall(string userId, string roomId)
        {
            if (_activeCalls.TryGetValue(roomId, out var callSession))
            {
                callSession.RejectCall(userId);
                Console.WriteLine($"Пользователь {userId} отклонил звонок в комнате {roomId}");
            }
        }

        public async Task EndCall(string roomId)
        {
            if (!_activeCalls.ContainsKey(roomId))
            {
                Console.WriteLine("Звонок не найден.");
                return;
            }

            bool success = await _voiceService.EndCallAsync(roomId);
            if (!success)
            {
                Console.WriteLine($"Ошибка завершения звонка в VoiceModul для {roomId}");
                return;
            }

            _activeCalls.TryRemove(roomId, out _);
            Console.WriteLine($"Звонок в комнате {roomId} завершен.");
        }
    }
}
