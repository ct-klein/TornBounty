using System.Text.Json;
using System.Text.Json.Serialization;

namespace TornBounty.Models;

public sealed class FlexibleStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.GetInt64().ToString(),
            JsonTokenType.Null => null,
            _ => reader.GetString()
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

public sealed class BountyResponse
{
    [JsonPropertyName("bounties_timestamp")]
    public long BountiesTimestamp { get; set; }

    [JsonPropertyName("bounties")]
    public List<Bounty> Bounties { get; set; } = [];

    [JsonPropertyName("_metadata")]
    public Metadata? Metadata { get; set; }
}

public sealed class Bounty
{
    [JsonPropertyName("target_id")]
    public int TargetId { get; set; }

    [JsonPropertyName("target_name")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? TargetName { get; set; }

    [JsonPropertyName("target_level")]
    public int TargetLevel { get; set; }

    [JsonPropertyName("lister_id")]
    public int? ListerId { get; set; }

    [JsonPropertyName("lister_name")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? ListerName { get; set; }

    [JsonPropertyName("reward")]
    public long Reward { get; set; }

    [JsonPropertyName("reason")]
    [JsonConverter(typeof(FlexibleStringConverter))]
    public string? Reason { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("is_anonymous")]
    public bool IsAnonymous { get; set; }

    [JsonPropertyName("valid_until")]
    public long ValidUntil { get; set; }
}

public sealed class Metadata
{
    [JsonPropertyName("links")]
    public PaginationLinks? Links { get; set; }
}

public sealed class PaginationLinks
{
    [JsonPropertyName("prev")]
    public string? Prev { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}
