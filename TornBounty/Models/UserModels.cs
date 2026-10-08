using System.Text.Json.Serialization;

namespace TornBounty.Models;

/// <summary>Response from /user?selections=profile,personalstats&amp;cat=popular for another player.</summary>
public sealed class UserLookupResponse
{
    [JsonPropertyName("profile")]
    public UserProfile? Profile { get; set; }

    [JsonPropertyName("personalstats")]
    public UserPopularStats? PersonalStats { get; set; }
}

public sealed class UserProfile
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    /// <summary>Rank name without the title, e.g. "Highly competent".</summary>
    [JsonPropertyName("rank")]
    public string? Rank { get; set; }

    [JsonPropertyName("status")]
    public UserStatus? Status { get; set; }
}

/// <summary>The subset of the "popular" personal stats category used for the battle stats estimate.</summary>
public sealed class UserPopularStats
{
    [JsonPropertyName("crimes")]
    public TotalStat? Crimes { get; set; }

    [JsonPropertyName("networth")]
    public TotalStat? Networth { get; set; }
}

public sealed class TotalStat
{
    [JsonPropertyName("total")]
    public long Total { get; set; }
}

public sealed class UserStatus
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }

    /// <summary>e.g. "Okay", "Hospital", "Jail", "Traveling", "Abroad", "Federal".</summary>
    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("color")]
    public string? Color { get; set; }

    /// <summary>Unix time the current state ends, if it has an end time.</summary>
    [JsonPropertyName("until")]
    public long? Until { get; set; }

    /// <summary>
    /// True if the player is attackable now: state "Okay", or a cached hospital/jail
    /// status whose <see cref="Until"/> time has already passed.
    /// </summary>
    public bool IsOkay
    {
        get
        {
            if (string.Equals(State, "Okay", StringComparison.OrdinalIgnoreCase))
                return true;

            var timedState = string.Equals(State, "Hospital", StringComparison.OrdinalIgnoreCase)
                || string.Equals(State, "Jail", StringComparison.OrdinalIgnoreCase);
            return timedState && Until > 0 && Until <= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}
