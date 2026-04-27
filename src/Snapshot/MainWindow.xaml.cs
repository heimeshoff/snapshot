using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Snapshot.Models;
using Snapshot.Services;
using Snapshot.Sources;
using Snapshot.Views;
using Wpf.Ui.Controls;

namespace Snapshot;

/// <summary>
/// Main window. Holds the sidebar navigation and a content host. Pages are
/// constructed lazily and cached for the session. Window geometry is restored
/// from <see cref="SettingsService"/> on construction and persisted on close.
/// </summary>
public partial class MainWindow : FluentWindow
{
    private readonly SettingsService _settings;
    private readonly SourceRepository _sourceRepository;
    private readonly BackupOrchestrator _orchestrator;

    private readonly Dictionary<string, UserControl> _pageCache = new();

    public MainWindow(
        SettingsService settings,
        SourceRepository sourceRepository,
        BackupOrchestrator orchestrator)
    {
        _settings = settings;
        _sourceRepository = sourceRepository;
        _orchestrator = orchestrator;

        InitializeComponent();

        RestoreWindowGeometry();

        // Default selection — Sources page first.
        NavigateTo("Sources");

        Closing += OnClosing;
    }

    private void RestoreWindowGeometry()
    {
        var ws = _settings.Current.Window;

        if (ws.Left.HasValue && ws.Top.HasValue && ws.Width.HasValue && ws.Height.HasValue)
        {
            // Trust the persisted rectangle if it lands on any workArea; otherwise fall through.
            var rect = new Rect(ws.Left.Value, ws.Top.Value, ws.Width.Value, ws.Height.Value);
            if (IsRectVisible(rect))
            {
                Left = ws.Left.Value;
                Top = ws.Top.Value;
                Width = ws.Width.Value;
                Height = ws.Height.Value;
                if (ws.Maximized) WindowState = WindowState.Maximized;
                return;
            }
        }

        var workArea = SystemParameters.WorkArea;
        Width = 1200;
        Height = 800;
        Left = workArea.Left + (workArea.Width - Width) / 2;
        Top = workArea.Top + (workArea.Height - Height) / 2;
    }

    private static bool IsRectVisible(Rect rect)
    {
        // Cheap heuristic — the primary monitor work area covers most cases for a
        // single-monitor user. Multi-monitor robustness is a follow-up.
        var work = SystemParameters.WorkArea;
        var intersection = Rect.Intersect(rect, work);
        return !intersection.IsEmpty && intersection.Width >= 100 && intersection.Height >= 100;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        try
        {
            _settings.Update(s =>
            {
                s.Window.Maximized = WindowState == WindowState.Maximized;
                var bounds = WindowState == WindowState.Maximized
                    ? RestoreBounds
                    : new Rect(Left, Top, Width, Height);
                s.Window.Left = bounds.Left;
                s.Window.Top = bounds.Top;
                s.Window.Width = bounds.Width;
                s.Window.Height = bounds.Height;
            });
        }
        catch (Exception ex)
        {
            // Saving geometry must never block shutdown.
            System.Diagnostics.Trace.TraceWarning("[MainWindow] Failed to persist window geometry: {0}", ex);
        }
    }

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PageContent is null) return;
        if (NavList.SelectedItem is ListBoxItem item && item.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    private void NavigateTo(string pageName)
    {
        if (PageContent is null) return;

        if (!_pageCache.TryGetValue(pageName, out var page))
        {
            page = pageName switch
            {
                "Sources" => new SourcesPage(_sourceRepository, _orchestrator),
                "Archives" => new ArchivesPage(),
                "Sync" => new SyncPage(),
                "Settings" => new SettingsPage(_settings),
                _ => null!,
            };

            if (page is not null) _pageCache[pageName] = page;
        }

        if (page is SourcesPage sources)
        {
            // Recompute Stale/Fresh labels every time the page becomes active —
            // staleness is a function of wall-clock time.
            sources.Refresh();
        }

        if (page is not null)
        {
            PageContent.Content = page;
        }
    }
}
