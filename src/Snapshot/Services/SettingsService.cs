using System.IO;
using System.Text.Json;
using Snapshot.Models;

namespace Snapshot.Services;

/// <summary>
/// Loads and saves <see cref="AppSettings"/> as JSON at
/// <c>%LOCALAPPDATA%\Snapshot\settings.json</c> (per ADR 0004).
///
/// First-run flow is silent: missing file means defaults are written without
/// prompting. A corrupt file is renamed to <c>settings.json.broken-&lt;utc&gt;</c>
/// and the loader proceeds with defaults.
///
/// Writes are atomic (write temp + rename over).
/// </summary>
public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private const int CurrentSchemaVersion = 1;

    private readonly string _settingsDir;
    private readonly string _settingsPath;
    private readonly object _sync = new();

    private AppSettings _current = new();

    public SettingsService()
        : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Snapshot"))
    {
    }

    /// <summary>
    /// Test-friendly constructor allowing the settings directory to be redirected.
    /// </summary>
    public SettingsService(string settingsDirectory)
    {
        _settingsDir = settingsDirectory;
        _settingsPath = Path.Combine(_settingsDir, "settings.json");
    }

    public AppSettings Current
    {
        get { lock (_sync) return _current; }
    }

    public string SettingsPath => _settingsPath;

    /// <summary>
    /// Loads settings from disk. Returns true if the file existed and parsed,
    /// false on first-run / corruption-recovery (defaults written).
    /// </summary>
    public bool Load()
    {
        Directory.CreateDirectory(_settingsDir);

        if (!File.Exists(_settingsPath))
        {
            lock (_sync)
            {
                _current = BuildDefaults();
            }
            Save();
            return false;
        }

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            if (loaded is null) throw new InvalidDataException("settings.json deserialized to null.");

            // Future schema bump handling: anything we don't know about → quarantine and reset.
            if (loaded.SchemaVersion > CurrentSchemaVersion)
            {
                throw new InvalidDataException($"settings.json schemaVersion {loaded.SchemaVersion} is newer than supported ({CurrentSchemaVersion}).");
            }

            // Backfill any missing required strings (defensive — older partially-written files).
            if (string.IsNullOrWhiteSpace(loaded.DatabasePath))
                loaded.DatabasePath = DefaultDatabasePath();
            if (string.IsNullOrWhiteSpace(loaded.DriveFolderPath))
                loaded.DriveFolderPath = DefaultDriveFolderPath();
            if (string.IsNullOrWhiteSpace(loaded.Theme))
                loaded.Theme = "system";

            lock (_sync)
            {
                _current = loaded;
            }
            return true;
        }
        catch (Exception ex)
        {
            // Corrupt or unreadable: rename to broken-<utc> and proceed with defaults.
            QuarantineCorruptFile(ex);
            lock (_sync)
            {
                _current = BuildDefaults();
            }
            Save();
            return false;
        }
    }

    /// <summary>
    /// Persists the current settings atomically (write to .tmp, rename over).
    /// </summary>
    public void Save()
    {
        Directory.CreateDirectory(_settingsDir);

        AppSettings snapshot;
        lock (_sync)
        {
            snapshot = _current;
        }

        var json = JsonSerializer.Serialize(snapshot, JsonOptions);
        var tmp = _settingsPath + ".tmp";
        File.WriteAllText(tmp, json);

        // File.Move with overwrite is atomic on NTFS for same-volume.
        if (File.Exists(_settingsPath))
            File.Replace(tmp, _settingsPath, destinationBackupFileName: null);
        else
            File.Move(tmp, _settingsPath);
    }

    /// <summary>
    /// Mutate-then-save helper. Caller passes a delegate that mutates the live
    /// settings object; the service serializes on save.
    /// </summary>
    public void Update(Action<AppSettings> mutator)
    {
        lock (_sync)
        {
            mutator(_current);
        }
        Save();
    }

    private void QuarantineCorruptFile(Exception cause)
    {
        try
        {
            var stamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
            var brokenPath = $"{_settingsPath}.broken-{stamp}";
            File.Move(_settingsPath, brokenPath, overwrite: false);
            System.Diagnostics.Trace.TraceWarning(
                "[SettingsService] settings.json was corrupt ({0}); quarantined to {1}.",
                cause.Message, brokenPath);
        }
        catch (Exception moveEx)
        {
            // Last-resort: delete it; we still want to boot.
            System.Diagnostics.Trace.TraceWarning(
                "[SettingsService] Could not quarantine corrupt settings.json ({0}); deleting. Original cause: {1}",
                moveEx.Message, cause.Message);
            try { File.Delete(_settingsPath); } catch { /* swallow */ }
        }
    }

    private static AppSettings BuildDefaults()
    {
        return new AppSettings
        {
            SchemaVersion = CurrentSchemaVersion,
            DatabasePath = DefaultDatabasePath(),
            DriveFolderPath = DefaultDriveFolderPath(),
            Theme = "system",
            Window = new WindowSettings(),
        };
    }

    public static string DefaultDatabasePath()
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(docs, "Snapshot", "snapshot.db");
    }

    public static string DefaultDriveFolderPath()
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(docs, "Snapshot", "drive");
    }
}
