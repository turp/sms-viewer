# auto-update

## Purpose

Provides in-app update detection and user-triggered update application. On startup the app silently checks GitHub Releases for a newer version and, if one is found, surfaces a non-blocking banner that lets the user restart into the new version or dismiss the notification for the session.

## Requirements

### Requirement: Update check on startup
The application SHALL check the GitHub Releases feed for a newer version once each time the app starts. The check SHALL run asynchronously and SHALL NOT block the UI from loading.

#### Scenario: New version available
- **WHEN** the app starts and the GitHub Releases feed contains a version newer than the running version
- **THEN** the update-available banner becomes visible showing the available version number

#### Scenario: Already on latest version
- **WHEN** the app starts and no newer version exists in the feed
- **THEN** no banner is shown and the app loads normally

#### Scenario: Update check fails
- **WHEN** the app starts and the update check fails (no network, rate limit, malformed feed)
- **THEN** no banner is shown, no error is displayed, and the app loads normally

### Requirement: Update available banner
The application SHALL display a non-blocking, dismissible banner at the top of the main window when an update is available. The banner SHALL show the available version number and two actions: "Restart to update" and "Dismiss".

#### Scenario: Banner is visible when update available
- **WHEN** `UpdateAvailable` is true on `MainWindowViewModel`
- **THEN** the banner is visible and displays the value of `AvailableVersion`

#### Scenario: Banner is hidden by default
- **WHEN** `UpdateAvailable` is false on `MainWindowViewModel`
- **THEN** the banner is not visible and takes no space in the layout

### Requirement: User-triggered restart and apply
The application SHALL restart and apply the staged update when the user clicks "Restart to update" in the banner.

#### Scenario: User confirms update
- **WHEN** the user clicks "Restart to update"
- **THEN** the application exits and relaunches into the new version

### Requirement: Dismiss update banner
The application SHALL hide the update banner for the remainder of the session when the user clicks "Dismiss".

#### Scenario: User dismisses banner
- **WHEN** the user clicks "Dismiss" in the update banner
- **THEN** the banner is hidden and does not reappear until the next app launch
