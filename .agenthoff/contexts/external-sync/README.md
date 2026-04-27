# External Sync

## Purpose
Mirror the Drive folder onto the encrypted USB Vault on demand, with a confirmable preview before any bytes move.

## Classification
Supporting

## Ubiquitous language
- **Vault** — the VeraCrypt-encrypted external drive.
- **Mount / Decrypt** — supplying the VeraCrypt password to make the Vault writable.
- **Sync preview** — collapsible tree of additions / deletions / modifications between the Drive folder and the Vault, with change counts at each branch.
- **Sync run** — the confirmed execution of a Sync preview.

## Notes
- Windows host. v1 uses robocopy (decided in `external-sync-001-veracrypt-and-robocopy`); content-hash deep-diff is deferred as a follow-up research spike.
- The Vault password is never stored.
- Sync is opportunistic, separate from the weekly backup ritual ("sometimes I do, sometimes I don't").
- Future Vault types beyond VeraCrypt-on-USB (Synology, other NAS) are anticipated but not in v1.

## Frontend gate
This BC has UI (the sync-preview tree, password prompt, confirm step). **Every frontend task captured for this BC must `depends_on: design-system-001-styleguide`.** No feature task that introduces or alters UI in this BC may begin work until the styleguide signoff is recorded.
