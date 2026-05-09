## ADDED Requirements

### Requirement: Theme Selection
The system SHALL provide exactly 4 built-in color themes: Ocean, Forest, Sunset, and Lavender. The user SHALL be able to select a theme from a picker in the toolbar. Each theme SHALL have both a light and a dark palette; the active palette SHALL be determined automatically by the OS theme variant.

#### Scenario: Default theme on first launch
- **WHEN** the user launches the app for the first time with no saved preference
- **THEN** the Ocean theme SHALL be active

#### Scenario: Selecting a theme applies it immediately
- **WHEN** the user selects a different theme from the theme picker
- **THEN** the sent message bubble colors SHALL update immediately without restarting the app

#### Scenario: All 4 themes are available
- **WHEN** the user opens the theme picker
- **THEN** it SHALL list Ocean, Forest, Sunset, and Lavender as selectable options

### Requirement: Theme Persistence
The system SHALL persist the user's selected theme across sessions.

#### Scenario: Theme restored on relaunch
- **WHEN** the user selects a theme and relaunches the app
- **THEN** the previously selected theme SHALL be active on next launch

#### Scenario: Missing or corrupt settings falls back to default
- **WHEN** the settings file is missing or cannot be read
- **THEN** the app SHALL launch with the Ocean (default) theme without error

### Requirement: Light and Dark Palettes
Each theme SHALL define distinct sent-bubble and text colors for both light and dark OS variants. The light/dark flavor SHALL switch automatically when the OS theme changes, without requiring user interaction.

#### Scenario: Light palette applied in light OS mode
- **WHEN** the OS is in light mode and a theme is active
- **THEN** the sent bubble SHALL use that theme's light palette colors

#### Scenario: Dark palette applied in dark OS mode
- **WHEN** the OS is in dark mode and a theme is active
- **THEN** the sent bubble SHALL use that theme's dark palette colors
