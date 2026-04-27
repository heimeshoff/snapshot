namespace Snapshot.Models;

/// <summary>
/// A single backup execution against a Source. Append-only domain log row,
/// per ADR 0006. The dashboard reads this log; Serilog handles application logs separately.
/// </summary>
public sealed class BackupRun
{
    public long Id { get; set; }
    public string SourceId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    /// <summary>"ok", "failed", or "partial".</summary>
    public string Outcome { get; set; } = "ok";

    public long? ArtifactSizeBytes { get; set; }
    public string? ErrorSummary { get; set; }
}
