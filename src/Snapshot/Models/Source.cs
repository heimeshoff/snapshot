namespace Snapshot.Models;

/// <summary>
/// A registered Source — a place that produces something Snapshot wants to back up.
/// Driven Sources are run by Snapshot. Witnessed Sources are observed via the
/// Drive folder (per ADR 0002). Staleness is computed from <see cref="LastBackupAt"/>
/// against <see cref="StalenessWindowMinutes"/>.
/// </summary>
public sealed class Source
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    /// <summary>"fake", "linux-ssh", "notion", "gmail", etc.</summary>
    public string Kind { get; set; } = string.Empty;

    /// <summary>"driven" or "witnessed" (per ADR 0002).</summary>
    public string Mode { get; set; } = "driven";

    public int StalenessWindowMinutes { get; set; }

    public DateTime? LastBackupAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsStale(DateTime nowUtc)
    {
        if (!LastBackupAt.HasValue) return true;
        var elapsed = nowUtc - LastBackupAt.Value;
        return elapsed.TotalMinutes >= StalenessWindowMinutes;
    }
}
