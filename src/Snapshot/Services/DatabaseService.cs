using System.IO;
using Microsoft.Data.Sqlite;

namespace Snapshot.Services;

/// <summary>
/// Owns the SQLite connection string and schema initialization. Per ADR 0004:
/// thin repository pattern, hand-written SQL, idempotent migrations on startup.
///
/// The schema covers <c>sources</c>, <c>backup_runs</c>, and <c>sync_runs</c>
/// (the domain log per ADR 0006). v1 spike — additive changes only.
/// </summary>
public sealed class DatabaseService
{
    private readonly string _databasePath;

    public DatabaseService(string databasePath)
    {
        _databasePath = databasePath;
    }

    public string DatabasePath => _databasePath;

    public string ConnectionString =>
        new SqliteConnectionStringBuilder
        {
            DataSource = _databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
        }.ToString();

    /// <summary>
    /// Ensures the parent directory exists, opens (or creates) the DB,
    /// and runs idempotent <c>CREATE TABLE IF NOT EXISTS</c> migrations.
    /// </summary>
    public void Initialize()
    {
        var dir = Path.GetDirectoryName(_databasePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        using var conn = OpenConnection();

        // Schema v1. Additive migrations only; column adds via ALTER TABLE in future.
        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS sources (
                id                          TEXT PRIMARY KEY,
                name                        TEXT NOT NULL,
                kind                        TEXT NOT NULL,
                mode                        TEXT NOT NULL,
                staleness_window_minutes    INTEGER NOT NULL,
                last_backup_at              TEXT NULL,
                created_at                  TEXT NOT NULL
            );
            """);

        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS backup_runs (
                id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                source_id           TEXT NOT NULL,
                started_at          TEXT NOT NULL,
                finished_at         TEXT NULL,
                outcome             TEXT NOT NULL,
                artifact_size_bytes INTEGER NULL,
                error_summary       TEXT NULL,
                FOREIGN KEY (source_id) REFERENCES sources(id)
            );
            """);

        ExecuteNonQuery(conn, """
            CREATE TABLE IF NOT EXISTS sync_runs (
                id                  INTEGER PRIMARY KEY AUTOINCREMENT,
                started_at          TEXT NOT NULL,
                finished_at         TEXT NULL,
                outcome             TEXT NOT NULL,
                error_summary       TEXT NULL
            );
            """);
    }

    public SqliteConnection OpenConnection()
    {
        var conn = new SqliteConnection(ConnectionString);
        conn.Open();
        return conn;
    }

    private static void ExecuteNonQuery(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }
}
