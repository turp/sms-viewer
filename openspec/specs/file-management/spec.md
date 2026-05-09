# file-management Specification

## Purpose
Defines the requirements for selecting and validating an XML backup file from the local filesystem, including surfacing file-access errors to the user.
## Requirements
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

### Requirement: Save XML File
The system SHALL provide a way for the user to choose a destination path and filename when saving an exported XML file.

#### Scenario: Save dialog opens with suggested name
- **WHEN** the system opens the save-file dialog for export
- **THEN** the dialog SHALL suggest a default filename (e.g., `sms-export.xml`) and filter to `.xml` files

#### Scenario: User selects save path
- **WHEN** the user confirms a path in the save-file dialog
- **THEN** the system SHALL write the exported XML to that path

#### Scenario: User cancels save dialog
- **WHEN** the user dismisses the save-file dialog without confirming
- **THEN** the system SHALL return a null path and SHALL NOT write any file

