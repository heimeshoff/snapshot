using System.IO;
using Snapshot.Models;
using Snapshot.Services;

namespace Snapshot.Sources;

/// <summary>
/// Runs a Driven backup against a single Source for the v1 spike. Touches a
/// placeholder file at the configured Drive folder, writes a <c>backup_runs</c>
/// row, and updates <c>last_backup_at</c> on the Source. Real adapters (linux-ssh,
/// notion, gmail) will plug in alongside the fake one in later tasks.
/// </summary>
public sealed class BackupOrchestrator
{
    private readonly SourceRepository _sources;
    private readonly BackupRunRepository _runs;
    private readonly Func<string> _driveFolderProvider;

    public BackupOrchestrator(
        SourceRepository sources,
        BackupRunRepository runs,
        Func<string> driveFolderProvider)
    {
        _sources = sources;
        _runs = runs;
        _driveFolderProvider = driveFolderProvider;
    }

    /// <summary>
    /// Performs a fake backup for the spike. Returns the resulting run row.
    /// </summary>
    public BackupRun RunBackup(Source source)
    {
        var startedAt = DateTime.UtcNow;
        var run = new BackupRun
        {
            SourceId = source.Id,
            StartedAt = startedAt,
            Outcome = "ok",
        };

        try
        {
            var driveFolder = _driveFolderProvider();
            Directory.CreateDirectory(driveFolder);

            // For the spike, "back up" means: touch a placeholder file named
            // after the Source. Real adapters will replace this with a real artifact.
            var artifactPath = Path.Combine(driveFolder, $"{source.Id}.placeholder");
            var contents = $"snapshot v1 placeholder for source {source.Id} ({source.Name}) — {startedAt:O}";
            File.WriteAllText(artifactPath, contents);

            var finishedAt = DateTime.UtcNow;
            run.FinishedAt = finishedAt;
            run.ArtifactSizeBytes = new FileInfo(artifactPath).Length;
            run.Outcome = "ok";

            _runs.Insert(run);
            _sources.UpdateLastBackupAt(source.Id, finishedAt);
            source.LastBackupAt = finishedAt;
        }
        catch (Exception ex)
        {
            run.FinishedAt = DateTime.UtcNow;
            run.Outcome = "failed";
            run.ErrorSummary = ex.Message;
            _runs.Insert(run);
        }

        return run;
    }
}
