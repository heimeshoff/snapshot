using System.IO;
using Snapshot.Models;
using Snapshot.Services;
using Snapshot.Sources;

namespace Snapshot.Tests;

/// <summary>
/// End-to-end-ish integration tests for the walking-skeleton spike. Spins up
/// the database, repositories, and orchestrator against a temp folder so the
/// real <c>%USERPROFILE%</c> isn't touched.
/// </summary>
public class WalkingSkeletonIntegrationTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _dbPath;
    private readonly string _drivePath;
    private readonly DatabaseService _db;
    private readonly SourceRepository _sources;
    private readonly BackupRunRepository _runs;
    private readonly BackupOrchestrator _orchestrator;
    private readonly SourceSeedingService _seeder;

    public WalkingSkeletonIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "snapshot-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _dbPath = Path.Combine(_tempDir, "snapshot.db");
        _drivePath = Path.Combine(_tempDir, "drive");

        _db = new DatabaseService(_dbPath);
        _db.Initialize();
        _sources = new SourceRepository(_db);
        _runs = new BackupRunRepository(_db);
        _orchestrator = new BackupOrchestrator(_sources, _runs, () => _drivePath);
        _seeder = new SourceSeedingService(_sources);
    }

    public void Dispose()
    {
        // SQLite needs the connection released before deleting; both repos open/close per call.
        try
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
        }
        catch { /* best-effort */ }
    }

    [Fact]
    public void Initialize_creates_schema_and_db_file()
    {
        Assert.True(File.Exists(_dbPath));
        // Tables should be queryable via the repos without throwing.
        Assert.Empty(_sources.ListAll());
    }

    [Fact]
    public void Seeder_inserts_fake_source_idempotently()
    {
        _seeder.EnsureSeeded();
        _seeder.EnsureSeeded(); // second call should not duplicate

        var all = _sources.ListAll();
        Assert.Single(all);
        var seeded = all[0];
        Assert.Equal(SourceSeedingService.FakeSourceId, seeded.Id);
        Assert.Equal("fake", seeded.Kind);
        Assert.Equal("driven", seeded.Mode);
        Assert.Equal(1, seeded.StalenessWindowMinutes);
        Assert.Null(seeded.LastBackupAt);
    }

    [Fact]
    public void Seeded_source_starts_stale_and_flips_to_fresh_after_backup()
    {
        _seeder.EnsureSeeded();
        var source = _sources.FindById(SourceSeedingService.FakeSourceId)!;
        Assert.True(source.IsStale(DateTime.UtcNow));

        var run = _orchestrator.RunBackup(source);

        Assert.Equal("ok", run.Outcome);
        Assert.NotNull(run.FinishedAt);
        Assert.True(run.ArtifactSizeBytes > 0);

        // Placeholder file appears at the configured Drive folder path.
        var artifactPath = Path.Combine(_drivePath, $"{source.Id}.placeholder");
        Assert.True(File.Exists(artifactPath));

        // Source row reflects the new last_backup_at and is no longer stale.
        var refreshed = _sources.FindById(SourceSeedingService.FakeSourceId)!;
        Assert.NotNull(refreshed.LastBackupAt);
        Assert.False(refreshed.IsStale(DateTime.UtcNow));
    }

    [Fact]
    public void Backup_writes_a_backup_runs_row()
    {
        _seeder.EnsureSeeded();
        var source = _sources.FindById(SourceSeedingService.FakeSourceId)!;

        _orchestrator.RunBackup(source);

        // Direct query — there is no list API yet; this is the spike's only check
        // that backup_runs is being populated.
        using var conn = _db.OpenConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM backup_runs WHERE source_id = $id;";
        cmd.Parameters.AddWithValue("$id", source.Id);
        var count = Convert.ToInt64(cmd.ExecuteScalar());
        Assert.Equal(1, count);
    }
}
