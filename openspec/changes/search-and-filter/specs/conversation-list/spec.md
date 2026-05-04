## MODIFIED Requirements

### Requirement: Conversation List Display
The system SHALL display a scrollable list of conversations in a left pane, sorted by the date of the most recent message descending. When filters are active, only conversations that satisfy all active filters SHALL be shown.

#### Scenario: Each conversation shows contact display name
- **WHEN** a conversation is listed
- **THEN** it SHALL display the contact name if available, otherwise the phone number address

#### Scenario: Each conversation shows last message preview
- **WHEN** a conversation is listed
- **THEN** it SHALL display a truncated preview of the most recent message body

#### Scenario: Each conversation shows last message date
- **WHEN** a conversation is listed
- **THEN** it SHALL display the human-readable date of the most recent message
