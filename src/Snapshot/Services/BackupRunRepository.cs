using System.Globalization;
using Snapshot.Models;

namespace Snapshot.Services;

/// <summary>
/// Append-mostly repository for <see cref="BackupRun"/> rows. Per ADR 0006,
/// this table is the dashboard's source of truth for "when did this Source
/// last back up successfully and how big was it."
/// </summary>
public sealed class BackupRunRepository
{
    private readonly DatabaseService _db;

    public BackupRunRepository(DatabaseService db)
    {
        _db = db;
    }

    public long Insert(BackupRun run)
    {
        using var conn = _db.OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO backup_runs (source_id, started_at, finished_at, outcome, artifact_size_bytes, error_summary)
            VALUES ($source, $started, $finished, $outcome, $size, $err);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$source", run.SourceId);
        cmd.Parameters.AddWithValue("$started", FormatUtc(run.StartedAt));
        cmd.Parameters.AddWithValue("$finished", (object?)FormatNullable(run.FinishedAt) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$outcome", run.Outcome);
        cmd.Parameters.AddWithValue("$size", (object?)run.ArtifactSizeBytes ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$err", (object?)run.ErrorSummary ?? DBNull.Value);

        var idObj = cmd.ExecuteScalar();
        run.Id = idObj is long l ? l : Convert.ToInt64(idObj, CultureInfo.InvariantCulture);
        return run.Id;
    }

    private static string FormatUtc(DateTime utc) =>
        utc.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);

    private static string? FormatNullable(DateTime? utc) =>
        utc.HasValue ? FormatUtc(utc.Value) : null;
}
