## ADDED Requirements

### Requirement: Open XML File
The system SHALL provide a way for the user to select an XML file from their local file system.

#### Scenario: User selects file
- **WHEN** the user clicks the "Open" button and selects a `.xml` file
- **THEN** the system SHALL attempt to load and parse that file

### Requirement: File Path Validation
The system SHALL ensure the selected file exists and is accessible.

#### Scenario: File not found
- **WHEN** the selected file is deleted or moved before parsing begins
- **THEN** the system SHALL show an error message to the user
