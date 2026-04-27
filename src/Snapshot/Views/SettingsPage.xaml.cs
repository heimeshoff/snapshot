using System.Windows.Controls;
using Snapshot.Services;

namespace Snapshot.Views;

/// <summary>
/// Read-only settings dashboard for the spike. Per the task scope, the
/// database path is shown but not editable; full path-editing UI is a
/// follow-up. Window geometry persists separately on close.
/// </summary>
public partial class SettingsPage : UserControl
{
    public SettingsPage(SettingsService settings)
    {
        InitializeComponent();

        var current = settings.Current;
        DatabasePathBox.Text = current.DatabasePath;
        DriveFolderPathBox.Text = current.DriveFolderPath;
        SettingsFilePathBox.Text = settings.SettingsPath;

        ThemeBox.SelectedIndex = current.Theme switch
        {
            "light" => 1,
            "dark" => 2,
            _ => 0,
        };
    }
}
