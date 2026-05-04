## ADDED Requirements

### Requirement: Conversation List Display
The system SHALL display a scrollable list of conversations in a left pane, sorted by the date of the most recent message descending.

#### Scenario: Each conversation shows contact display name
- **WHEN** a conversation is listed
- **THEN** it SHALL display the contact name if available, otherwise the phone number address

#### Scenario: Each conversation shows last message preview
- **WHEN** a conversation is listed
- **THEN** it SHALL display a truncated preview of the most recent message body

#### Scenario: Each conversation shows last message date
- **WHEN** a conversation is listed
- **THEN** it SHALL display the human-readable date of the most recent message

### Requirement: Conversation Selection
The system SHALL allow the user to select a conversation from the list.

#### Scenario: Selecting a conversation loads the thread
- **WHEN** the user clicks a conversation in the left pane
- **THEN** the right pane SHALL display the full message thread for that conversation

#### Scenario: No conversation selected on initial load
- **WHEN** the file has finished loading
- **THEN** the right pane SHALL be empty until the user selects a conversation
