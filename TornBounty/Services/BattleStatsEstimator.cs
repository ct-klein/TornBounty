using TornBounty.Models;

namespace TornBounty.Services;

/// <summary>
/// Estimates a player's battle stats total from public profile data.
/// A player's rank is the sum of "triggers" reached for level, crimes, networth and battle stats,
/// so subtracting the first three from the rank leaves the battle stats bracket.
/// This is the same approach TornTools uses; it is an estimate, not a spy result.
/// </summary>
public static class BattleStatsEstimator
{
    private static readonly int[] LevelTriggers = [2, 6, 11, 26, 31, 50, 71, 100];
    private static readonly int[] CrimeTriggers = [100, 5_000, 10_000, 20_000, 30_000, 50_000];
    private static readonly long[] NetworthTriggers = [5_000_000, 50_000_000, 500_000_000, 5_000_000_000, 50_000_000_000];

    // The brackets overlap by design; rank thresholds aren't exact stat values
    public static readonly IReadOnlyList<string> TierLabels =
        ["< 2k", "2k - 25k", "20k - 250k", "200k - 2.5M", "2M - 25M", "20M - 250M", "> 200M"];

    private static readonly string[] Ranks =
    [
        "Absolute beginner", "Beginner", "Inexperienced", "Rookie", "Novice", "Below average", "Average",
        "Reasonable", "Above average", "Competent", "Highly competent", "Veteran", "Distinguished",
        "Highly distinguished", "Professional", "Star", "Master", "Outstanding", "Celebrity", "Supreme",
        "Idolized", "Champion", "Heroic", "Legendary", "Elite", "Invincible"
    ];

    /// <summary>Returns the index into <see cref="TierLabels"/>, or null if the data doesn't fit the model.</summary>
    public static int? EstimateTier(UserLookupResponse lookup)
    {
        var profile = lookup.Profile;
        var stats = lookup.PersonalStats;
        if (profile?.Rank is null || stats?.Crimes is null || stats.Networth is null)
            return null;

        var rankIndex = Array.FindIndex(Ranks, r => string.Equals(r, profile.Rank, StringComparison.OrdinalIgnoreCase));
        if (rankIndex < 0)
            return null;

        var tier = rankIndex
            - LevelTriggers.Count(t => t <= profile.Level)
            - CrimeTriggers.Count(t => t <= stats.Crimes.Total)
            - NetworthTriggers.Count(t => t <= stats.Networth.Total);

        return tier >= 0 && tier < TierLabels.Count ? tier : null;
    }
}
