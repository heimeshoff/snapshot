using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Snapshot.Models;
using Snapshot.Services;
using Snapshot.Sources;

namespace Snapshot.Views;

/// <summary>
/// Dashboard view owned by the Sources BC. Lists every registered Source with
/// its computed Stale/Fresh state and a "Back up now" action wired to
/// <see cref="BackupOrchestrator"/>.
/// </summary>
public partial class SourcesPage : UserControl
{
    private readonly SourceRepository _sourceRepository;
    private readonly BackupOrchestrator _orchestrator;

    private readonly ObservableCollection<SourceViewModel> _items = new();

    public SourcesPage(SourceRepository sourceRepository, BackupOrchestrator orchestrator)
    {
        _sourceRepository = sourceRepository;
        _orchestrator = orchestrator;

        InitializeComponent();
        SourceList.ItemsSource = _items;

        Refresh();
    }

    public void Refresh()
    {
        _items.Clear();
        var nowUtc = DateTime.UtcNow;
        foreach (var source in _sourceRepository.ListAll())
        {
            _items.Add(SourceViewModel.From(source, nowUtc));
        }
    }

    private void BackUpNow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement fe) return;
        if (fe.Tag is not string sourceId) return;

        var source = _sourceRepository.FindById(sourceId);
        if (source is null) return;

        _orchestrator.RunBackup(source);
        Refresh();
    }
}

/// <summary>View-model row for a single Source on the dashboard.</summary>
public sealed class SourceViewModel
{
    public string SourceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string KindLabel { get; init; } = string.Empty;
    public string LastBackupLabel { get; init; } = string.Empty;
    public string StateLabel { get; init; } = string.Empty;
    public Brush StateBackgroundBrush { get; init; } = Brushes.Transparent;
    public Brush StateForegroundBrush { get; init; } = Brushes.Black;
    public bool CanBackUp { get; init; }

    private static readonly Brush StaleBg = new SolidColorBrush(Color.FromRgb(0xFD, 0xE2, 0xE4));
    private static readonly Brush StaleFg = new SolidColorBrush(Color.FromRgb(0x99, 0x1B, 0x1F));
    private static readonly Brush FreshBg = new SolidColorBrush(Color.FromRgb(0xDF, 0xF4, 0xE3));
    private static readonly Brush FreshFg = new SolidColorBrush(Color.FromRgb(0x1E, 0x6F, 0x2A));

    static SourceViewModel()
    {
        StaleBg.Freeze();
        StaleFg.Freeze();
        FreshBg.Freeze();
        FreshFg.Freeze();
    }

    public static SourceViewModel From(Source source, DateTime nowUtc)
    {
        var stale = source.IsStale(nowUtc);
        return new SourceViewModel
        {
            SourceId = source.Id,
            Name = source.Name,
            KindLabel = $"{source.Kind} / {source.Mode}",
            LastBackupLabel = source.LastBackupAt.HasValue
                ? $"last backup {source.LastBackupAt.Value.ToLocalTime():yyyy-MM-dd HH:mm}"
                : "never backed up",
            StateLabel = stale ? "STALE" : "FRESH",
            StateBackgroundBrush = stale ? StaleBg : FreshBg,
            StateForegroundBrush = stale ? StaleFg : FreshFg,
            // Witnessed Sources can't be driven from Snapshot (per ADR 0002), so the button is gated.
            CanBackUp = source.Mode == "driven",
        };
    }
}
