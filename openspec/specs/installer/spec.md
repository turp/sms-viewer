# installer

## Purpose

Defines the CI release pipeline artifacts and publishing steps required to distribute the application via Velopack. Covers Windows NSIS installer, Linux AppImage, version derivation from git tags, and publishing of the GitHub Release alongside the Velopack feed so that the in-app updater can discover new versions.

## Requirements

### Requirement: Windows installer artifact
The CI release pipeline SHALL produce a Velopack NSIS `.exe` installer for Windows when a version tag is pushed. The installer SHALL support silent install (`/S` flag) and standard uninstall via Windows Add/Remove Programs.

#### Scenario: Windows release build
- **WHEN** a tag matching `v*.*.*` is pushed and the `build-windows` CI job runs
- **THEN** a `.exe` installer artifact is produced and uploaded to the GitHub Release

#### Scenario: Silent install
- **WHEN** the installer is invoked with the `/S` flag
- **THEN** the application installs without any UI prompts

### Requirement: Linux AppImage artifact
The CI release pipeline SHALL produce a Velopack `.AppImage` for Linux when a version tag is pushed. The AppImage SHALL be executable without root privileges.

#### Scenario: Linux release build
- **WHEN** a tag matching `v*.*.*` is pushed and the `build-linux` CI job runs
- **THEN** a `.AppImage` artifact is produced and uploaded to the GitHub Release

#### Scenario: No root required
- **WHEN** a user runs the AppImage on a compatible Linux system
- **THEN** the application launches without requiring elevated permissions

### Requirement: Version derived from git tag
The release version embedded in all installer artifacts SHALL be derived from the git tag that triggered the CI run. No version SHALL be hard-coded in the project file.

#### Scenario: Tag v1.2.3 produces version 1.2.3
- **WHEN** the CI workflow is triggered by tag `v1.2.3`
- **THEN** the packaged installer reports application version `1.2.3`

### Requirement: GitHub Release and Velopack feed published
The CI release pipeline SHALL create a GitHub Release and publish the Velopack feed file (`releases.json`) alongside the installer artifacts so that the in-app updater can discover new versions.

#### Scenario: Release artifacts published
- **WHEN** both platform build jobs complete successfully
- **THEN** a GitHub Release exists containing the `.exe`, `.AppImage`, and `releases.json` feed file

#### Scenario: Feed accessible to update check
- **WHEN** the running app calls `UpdateManager.CheckForUpdatesAsync()`
- **THEN** the manager resolves the latest version from `releases.json` on the GitHub Release
