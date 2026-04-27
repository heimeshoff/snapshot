using System.Globalization;
using Microsoft.Data.Sqlite;
using Snapshot.Models;

namespace Snapshot.Services;

/// <summary>
/// Hand-written SQL repository for <see cref="Source"/> records. Per ADR 0004,
/// no ORM. Timestamps are stored as ISO-8601 UTC strings.
/// </summary>
public sealed class SourceRepository
{
    private readonly DatabaseService _db;

    public SourceRepository(DatabaseService db)
    {
        _db = db;
    }

    public IReadOnlyList<Source> ListAll()
    {
        using var conn = _db.OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, name, kind, mode, staleness_window_minutes, last_backup_at, created_at
              FROM sources
             ORDER BY created_at;
            """;

        var results = new List<Source>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(MapRow(reader));
        }
        return results;
    }

    public Source? FindById(string id)
    {
        using var conn = _db.OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT id, name, kind, mode, staleness_window_minutes, last_backup_at, created_at
              FROM sources
             WHERE id = $id;
            """;
        cmd.Parameters.AddWithValue("$id", id);

        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapRow(reader) : null;
    }

    public void Insert(Source source)
    {
        using var conn = _db.OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO sources (id, name, kind, mode, staleness_window_minutes, last_backup_at, created_at)
            VALUES ($id, $name, $kind, $mode, $stale, $last, $created);
            """;
        cmd.Parameters.AddWithValue("$id", source.Id);
        cmd.Parameters.AddWithValue("$name", source.Name);
        cmd.Parameters.AddWithValue("$kind", source.Kind);
        cmd.Parameters.AddWithValue("$mode", source.Mode);
        cmd.Parameters.AddWithValue("$stale", source.StalenessWindowMinutes);
        cmd.Parameters.AddWithValue("$last", (object?)FormatNullable(source.LastBackupAt) ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$created", FormatUtc(source.CreatedAt));
        cmd.ExecuteNonQuery();
    }

    public void UpdateLastBackupAt(string sourceId, DateTime lastBackupAtUtc)
    {
        using var conn = _db.OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            UPDATE sources
               SET last_backup_at = $last
             WHERE id = $id;
            """;
        cmd.Parameters.AddWithValue("$id", sourceId);
        cmd.Parameters.AddWithValue("$last", FormatUtc(lastBackupAtUtc));
        cmd.ExecuteNonQuery();
    }

    private static Source MapRow(SqliteDataReader reader)
    {
        return new Source
        {
            Id = reader.GetString(0),
            Name = reader.GetString(1),
            Kind = reader.GetString(2),
            Mode = reader.GetString(3),
            StalenessWindowMinutes = reader.GetInt32(4),
            LastBackupAt = reader.IsDBNull(5) ? null : ParseUtc(reader.GetString(5)),
            CreatedAt = ParseUtc(reader.GetString(6)),
        };
    }

    private static string FormatUtc(DateTime utc) =>
        utc.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);

    private static string? FormatNullable(DateTime? utc) =>
        utc.HasValue ? FormatUtc(utc.Value) : null;

    private static DateTime ParseUtc(string s) =>
        DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind).ToUniversalTime();
}
