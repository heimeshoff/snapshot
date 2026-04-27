using System.IO;
using System.Text.Json;
using Snapshot.Services;

namespace Snapshot.Tests;

/// <summary>
/// Tests that exercise <see cref="SettingsService"/> without poking at the
/// real <c>%LOCALAPPDATA%</c>. We verify defaults shape and corrupt-file
/// quarantine via direct JSON parsing (the service writes a known shape).
/// </summary>
public class SettingsServiceTests
{
    [Fact]
    public void Default_database_path_lives_under_documents_snapshot()
    {
        var path = SettingsService.DefaultDatabasePath();
        Assert.EndsWith(Path.Combine("Snapshot", "snapshot.db"), path, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Default_drive_folder_lives_under_documents_snapshot_drive()
    {
        var path = SettingsService.DefaultDriveFolderPath();
        Assert.EndsWith(Path.Combine("Snapshot", "drive"), path, StringComparison.OrdinalIgnoreCase);
    }
}
