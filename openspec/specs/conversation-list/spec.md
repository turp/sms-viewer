# conversation-list Specification

## Purpose
TBD - created by archiving change group-by-conversation. Update Purpose after archive.
## Requirements
### Requirement: Conversation List Display
The system SHALL display a scrollable list of conversations in a left pane, sorted by the date of the most recent message descending. When filters are active, only conversations that satisfy all active filters SHALL be shown. The conversation list SHALL be populated from lightweight summary data and SHALL not require preloading every message thread before the list becomes available.

#### Scenario: Each conversation shows contact display name
- **WHEN** a conversation is listed
- **THEN** it SHALL display the contact name if available, otherwise the phone number address

#### Scenario: Unknown sentinel falls back to phone number
- **WHEN** a conversation's contact name is `(Unknown)`
- **THEN** the conversation list SHALL display the phone number address instead of `(Unknown)`

#### Scenario: Each conversation shows last message preview
- **WHEN** a conversation is listed
- **THEN** it SHALL display a truncated preview of the most recent message body

#### Scenario: Each conversation shows last message date
- **WHEN** a conversation is listed
- **THEN** it SHALL display the human-readable date of the most recent message

#### Scenario: Large file can surface conversation summaries before thread data
- **WHEN** the user opens a large XML backup file
- **THEN** the system SHALL make the conversation list available from summary data without requiring full thread data for all conversations to already be loaded

### Requirement: Conversation Selection
The system SHALL allow the user to select a conversation from the list.

#### Scenario: Selecting a conversation loads the thread
- **WHEN** the user clicks a conversation in the left pane
- **THEN** the right pane SHALL load and display the message thread for that conversation on demand

#### Scenario: No conversation selected on initial load
- **WHEN** the file has finished loading
- **THEN** the right pane SHALL be empty until the user selects a conversation

