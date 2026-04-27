using Snapshot.Models;

namespace Snapshot.Tests;

public class SourceStalenessTests
{
    [Fact]
    public void Source_with_no_last_backup_is_stale()
    {
        var source = new Source { StalenessWindowMinutes = 1 };
        Assert.True(source.IsStale(DateTime.UtcNow));
    }

    [Fact]
    public void Source_within_window_is_fresh()
    {
        var now = DateTime.UtcNow;
        var source = new Source
        {
            StalenessWindowMinutes = 60,
            LastBackupAt = now.AddMinutes(-10),
        };
        Assert.False(source.IsStale(now));
    }

    [Fact]
    public void Source_past_window_is_stale()
    {
        var now = DateTime.UtcNow;
        var source = new Source
        {
            StalenessWindowMinutes = 1,
            LastBackupAt = now.AddMinutes(-5),
        };
        Assert.True(source.IsStale(now));
    }
}
