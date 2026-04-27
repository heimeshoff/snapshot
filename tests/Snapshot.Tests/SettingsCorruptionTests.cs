using System.IO;
using Snapshot.Services;

namespace Snapshot.Tests;

/// <summary>
/// Verifies the corrupt-file quarantine behavior required by ADR 0004 and the
/// walking-skeleton acceptance criteria: a malformed <c>settings.json</c> must
/// be renamed to <c>settings.json.broken-&lt;utc&gt;</c> and the loader must
/// proceed with defaults silently.
/// </summary>
public class SettingsCorruptionTests : IDisposable
{
    private readonly string _tempDir;

    public SettingsCorruptionTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "snapshot-settings-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        try { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true); } catch { }
    }

    [Fact]
    public void First_run_writes_default_settings_file_silently()
    {
        var service = new SettingsService(_tempDir);
        var loadedExisting = service.Load();

        Assert.False(loadedExisting);
        Assert.True(File.Exists(Path.Combine(_tempDir, "settings.json")));
        Assert.Equal(1, service.Current.SchemaVersion);
        Assert.False(string.IsNullOrEmpty(service.Current.DatabasePath));
    }

    [Fact]
    public void Corrupt_file_is_quarantined_and_defaults_are_used()
    {
        var settingsPath = Path.Combine(_tempDir, "settings.json");
        File.WriteAllText(settingsPath, "{ this is not valid JSON");

        var service = new SettingsService(_tempDir);
        service.Load();

        // Original is gone; a broken-<utc> sibling exists.
        Assert.False(File.Exists(settingsPath) && File.ReadAllText(settingsPath).StartsWith("{ this is not valid JSON"));

        var brokenFiles = Directory.GetFiles(_tempDir, "settings.json.broken-*");
        Assert.Single(brokenFiles);

        // A fresh defaults file is in place.
        Assert.True(File.Exists(settingsPath));
        Assert.Equal(1, service.Current.SchemaVersion);
    }

    [Fact]
    public void Round_trip_persists_window_geometry()
    {
        var service = new SettingsService(_tempDir);
        service.Load();
        service.Update(s =>
        {
            s.Window.Left = 120;
            s.Window.Top = 80;
            s.Window.Width = 1024;
            s.Window.Height = 720;
            s.Window.Maximized = false;
        });

        var reloaded = new SettingsService(_tempDir);
        reloaded.Load();

        Assert.Equal(120, reloaded.Current.Window.Left);
        Assert.Equal(80, reloaded.Current.Window.Top);
        Assert.Equal(1024, reloaded.Current.Window.Width);
        Assert.Equal(720, reloaded.Current.Window.Height);
        Assert.False(reloaded.Current.Window.Maximized);
    }
}
