using AuthApi.Models.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

[ApiController]
[Route("api/call")]
public class CallController : ControllerBase
{
    private const string VoiceServerUrl = "http://147.45.175.135:5001/voice"; // Адрес сервера голосовой связи

    private readonly HttpClient _httpClient;

    public CallController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartCall([FromBody] CallRequest request, [FromServices] IHubContext<VoiceHub> voiceHub)
    {
        // Отправляем команду на сервер голосовой связи
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{VoiceServerUrl}/start-call", content);

        if (response.IsSuccessStatusCode)
        {
            foreach (var userId in request.Users)
            {
                await voiceHub.Clients.User(userId.ToString()).SendAsync("IncomingCall", request.RoomId, request.Users[0]); // Отправляем звонок всем пользователям
            }

            return Ok("Звонок начат");
        }

        return StatusCode((int)response.StatusCode, "Ошибка при создании звонка");
    }


    [HttpPost("confirm")]
    public async Task<IActionResult> ConfirmCall([FromBody] CallConfirmation request)
    {
        // Подтверждение звонка, добавляем пользователя в комнату
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{VoiceServerUrl}/confirm-call", content);

        if (response.IsSuccessStatusCode)
            return Ok("Пользователь подключён");

        return StatusCode((int)response.StatusCode, "Ошибка подключения пользователя");
    }

    [HttpPost("end")]
    public async Task<IActionResult> EndCall([FromBody] CallEndRequest request)
    {
        // Завершаем звонок
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{VoiceServerUrl}/end-call", content);

        if (response.IsSuccessStatusCode)
            return Ok("Звонок завершён");

        return StatusCode((int)response.StatusCode, "Ошибка завершения звонка");
    }
}

// Модель данных для запросов
public class CallRequest
{
    public string RoomId { get; set; }
    public List<string> Users { get; set; } // Список ID пользователей
}

public class CallConfirmation
{
    public string RoomId { get; set; }
    public string UserId { get; set; }
}

public class CallEndRequest
{
    public string RoomId { get; set; }
}
