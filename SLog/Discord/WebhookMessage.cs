using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SLog.Discord;

public class WebhookMessage
{
    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonProperty("username")]
    public string? Username { get; set; }

    [JsonProperty("avatar_url")]
    public string? AvatarUrl { get; set; }

}

