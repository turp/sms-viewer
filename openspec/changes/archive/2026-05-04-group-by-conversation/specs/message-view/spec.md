## REMOVED Requirements

### Requirement: Message List Display
**Reason**: Replaced by the Conversation List Display and Message Thread Display requirements introduced in the conversation-list and conversation-thread capabilities.
**Migration**: The flat chronological message list is removed. Users now browse messages via the two-pane conversation UI.

## MODIFIED Requirements

### Requirement: Responsive UI
The system SHALL remain responsive during the parsing and grouping of large files.

#### Scenario: Progressive loading
- **WHEN** a large XML file is being parsed and grouped
- **THEN** the UI SHALL show a progress indicator until all conversations are ready, then populate the conversation list
