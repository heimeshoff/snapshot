namespace Snapshot.Models;

/// <summary>
/// Per-machine application settings persisted to
/// <c>%LOCALAPPDATA%\Snapshot\settings.json</c>. Schema v1 per ADR 0004.
/// </summary>
public sealed class AppSettings
{
    public int SchemaVersion { get; set; } = 1;

    /// <summary>Absolute path to the operational SQLite database.</summary>
    public string DatabasePath { get; set; } = string.Empty;

    /// <summary>Absolute path to the Drive folder where backup artifacts land.</summary>
    public string DriveFolderPath { get; set; } = string.Empty;

    public WindowSettings Window { get; set; } = new();

    /// <summary>"light", "dark", or "system".</summary>
    public string Theme { get; set; } = "system";
}

public sealed class WindowSettings
{
    public double? Left { get; set; }
    public double? Top { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public bool Maximized { get; set; }
}
