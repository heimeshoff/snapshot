using Snapshot.Models;
using Snapshot.Services;

namespace Snapshot.Sources;

/// <summary>
/// Ensures the seeded fake Source exists in the registry. The walking-skeleton
/// spike registers exactly one fake Driven Source so the dashboard has something
/// to render and act on. Idempotent — safe to call on every startup.
/// </summary>
public sealed class SourceSeedingService
{
    public const string FakeSourceId = "fake-001";

    private readonly SourceRepository _sources;

    public SourceSeedingService(SourceRepository sources)
    {
        _sources = sources;
    }

    public void EnsureSeeded()
    {
        var existing = _sources.FindById(FakeSourceId);
        if (existing is not null) return;

        _sources.Insert(new Source
        {
            Id = FakeSourceId,
            Name = "Fake source (spike)",
            Kind = "fake",
            Mode = "driven",
            StalenessWindowMinutes = 1,
            LastBackupAt = null,
            CreatedAt = DateTime.UtcNow,
        });
    }
}
