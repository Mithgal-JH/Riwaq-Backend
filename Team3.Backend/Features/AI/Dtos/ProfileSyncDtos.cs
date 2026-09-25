using System.Text.Json.Serialization;

namespace Team3.Backend.Features.AI.Dtos;

public sealed class ProfileSyncRequest
{
    public string SyncType { get; set; } = string.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ProfileSyncItem>? Profiles { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? ProfileIds { get; set; }
}

public sealed class ProfileSyncItem
{
    public string ProfileId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public List<string> Skills { get; set; } = [];

    public List<string> Interests { get; set; } = [];

    public string? LearningDirection { get; set; }

    public string? Bio { get; set; }
}

public sealed class ProfileSyncResponse
{
    public string SyncStatus { get; set; } = string.Empty;

    public int ProfilesIndexed { get; set; }

    public string IndexVersion { get; set; } = string.Empty;
}
