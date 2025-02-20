using System.Text;
using System.Text.Json;

public class VoiceService
{
    private const string VoiceServerUrl = "http://147.45.175.135:5001/voice";
    private readonly HttpClient _httpClient;

    public VoiceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> StartCallAsync(string roomId, string callerId, List<string> participants)
    {
        var request = new
        {
            CallerId = callerId,
            RoomId = roomId,
            Users = participants
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{VoiceServerUrl}/start-call", content);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ConfirmCallAsync(string roomId, string userId)
    {
        var request = new
        {
            RoomId = roomId,
            UserId = userId
        };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{VoiceServerUrl}/confirm-call", content);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EndCallAsync(string roomId)
    {
        var request = new { RoomId = roomId };

        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{VoiceServerUrl}/end-call", content);

        return response.IsSuccessStatusCode;
    }
}
