using System.IO;
using System.Windows;
using Snapshot.Services;
using Snapshot.Sources;
using Wpf.Ui.Appearance;

namespace Snapshot;

/// <summary>
/// Composition root. Per ADR 0004, settings load before the database; per ADR 0006,
/// the domain log lives in SQLite alongside the registry. The walking-skeleton
/// spike wires everything in code (no DI container) to keep moving parts visible.
/// </summary>
public partial class App : Application
{
    private void OnStartup(object sender, StartupEventArgs e)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            System.Diagnostics.Trace.TraceError("[App] Unhandled domain exception: {0}", ex);
        };

        DispatcherUnhandledException += (_, args) =>
        {
            System.Diagnostics.Trace.TraceError("[App] Unhandled UI exception: {0}", args.Exception);
            MessageBox.Show(
                $"Snapshot encountered an error:\n\n{args.Exception.Message}",
                "Snapshot",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            args.Handled = true;
        };

        try
        {
            StartupCore();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Snapshot failed to start:\n\n{ex.Message}\n\n{ex.StackTrace}",
                "Snapshot — startup error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private void StartupCore()
    {
        // 1. Settings (silent first-run, corrupt-quarantine).
        var settings = new SettingsService();
        settings.Load();

        // 2. Make sure the default DB folder exists. If we can't create it, fall back
        //    to a folder picker — but the happy path stays silent (per ADR 0004).
        EnsureDatabaseFolderOrFallback(settings);

        // 3. Operational database — open / migrate.
        var db = new DatabaseService(settings.Current.DatabasePath);
        db.Initialize();

        // 4. Repositories.
        var sourceRepo = new SourceRepository(db);
        var backupRunRepo = new BackupRunRepository(db);

        // 5. Seed the fake Source for the spike.
        var seeder = new SourceSeedingService(sourceRepo);
        seeder.EnsureSeeded();

        // 6. Sources BC services.
        var orchestrator = new BackupOrchestrator(
            sourceRepo,
            backupRunRepo,
            driveFolderProvider: () => settings.Current.DriveFolderPath);

        // 7. Apply theme (system / light / dark — see ADR 0004).
        ApplyTheme(settings.Current.Theme);

        // 8. Show main window.
        var mainWindow = new MainWindow(settings, sourceRepo, orchestrator);
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    private static void ApplyTheme(string theme)
    {
        switch (theme)
        {
            case "light":
                ApplicationThemeManager.Apply(ApplicationTheme.Light);
                break;
            case "dark":
                ApplicationThemeManager.Apply(ApplicationTheme.Dark);
                break;
            default:
                ApplicationThemeManager.ApplySystemTheme();
                break;
        }
    }

    private static void EnsureDatabaseFolderOrFallback(SettingsService settings)
    {
        var path = settings.Current.DatabasePath;
        var dir = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(dir)) return;

        try
        {
            Directory.CreateDirectory(dir);
        }
        catch (Exception ex)
        {
            // Per ADR 0004 — fall back ONLY when default folder can't be created.
            // For the spike, surface a message instead of a folder-picker; the
            // proper picker UX lands when the editable Settings UI does.
            MessageBox.Show(
                "Snapshot's default database folder is unavailable. Please choose a different location in Settings on next start.\n\n"
                    + $"Tried: {dir}\nReason: {ex.Message}",
                "Snapshot — database folder",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            throw;
        }
    }
}
