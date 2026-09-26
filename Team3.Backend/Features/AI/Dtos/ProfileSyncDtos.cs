using System.Text.Json.Serialization;

namespace Team3.Backend.Features.AI.Dtos;

public sealed class ProfileSyncRequest
{
    [JsonPropertyName("sync_type")]
    public string SyncType { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("profiles")]
    public List<ProfileSyncItem>? Profiles { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("profile_ids")]
    public List<string>? ProfileIds { get; set; }
}

public sealed class ProfileSyncItem
{
    [JsonPropertyName("profile_id")]
    public string ProfileId { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("skills")]
    public List<string> Skills { get; set; } = [];

    [JsonPropertyName("interests")]
    public List<string> Interests { get; set; } = [];

    [JsonPropertyName("learning_direction")]
    public string? LearningDirection { get; set; }

    [JsonPropertyName("bio")]
    public string? Bio { get; set; }
}

public sealed class ProfileSyncResponse
{
    public string SyncStatus { get; set; } = string.Empty;

    public int ProfilesIndexed { get; set; }

    public string IndexVersion { get; set; } = string.Empty;
}
