using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SLog.Discord;

public class DiscordWebhook
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public DiscordWebhook(ulong id, string token) : this($"https://discordapp.com/api/webhooks/{id}/{token}") { }

    public DiscordWebhook(string webhookUrl)
    {
        _httpClient = new HttpClient();
        _webhookUrl = webhookUrl;
    }

    public Task<HttpResponseMessage> Send(string content)
    {
        return Send(new WebhookMessage() { Content = content });
    }

    public async Task<HttpResponseMessage> Send(WebhookMessage msg)
    {
        var content = new StringContent(JsonConvert.SerializeObject(msg), Encoding.UTF8, "application/json");

        var resp = await _httpClient.PostAsync(_webhookUrl, content);
        return resp;
    }

}
