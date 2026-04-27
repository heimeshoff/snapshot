namespace Snapshot.ExternalSync;

// Bounded context: ExternalSync.
//
// Owns the Sync ritual — VeraCrypt mount, robocopy mirror, dismount — that copies
// the Drive folder into the Vault. v1 spike ships a stub view only (see SyncPage).
// Real implementation lands behind external-sync-001-veracrypt-and-robocopy.
//
// This file is a marker so the BC folder isn't empty; concrete services land here later.
internal static class _Marker { }
