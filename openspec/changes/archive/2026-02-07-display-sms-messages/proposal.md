## Why

Users need a way to view their backed-up SMS and MMS messages in a readable format. Currently, the data is stored in raw XML files which are difficult to navigate manually. Providing a viewer will allow users to easily browse their message history.

## What Changes

- Add a service to parse SMS/MMS XML files using streaming (XmlReader).
- Implement a user interface to display a list of messages with sender, date, and content.
- Add file picker functionality to allow users to load their own XML backup files.

## Capabilities

### New Capabilities
- `xml-parsing`: logic to stream and parse SMS/MMS messages from XML backups, adhering to the provided schema.
- `message-view`: UI components based on Avalonia to display message sequences, including contact details and timestamps.
- `file-management`: Service to handle local file system access for selecting and reading backup files.

### Modified Capabilities
- None

## Impact

- **Models**: New domain models for `SmsMessage` and `MmsMessage`.
- **ViewModels**: Update `MainWindowViewModel` to handle message loading and state.
- **Views**: Update `MainWindow` to include message display areas and file opening controls.
- **Dependencies**: No new external dependencies required (System.Xml is part of .NET).
