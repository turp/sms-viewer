## ADDED Requirements

### Requirement: Message List Display
The system SHALL display a scrollable list of messages sorted by date descending.

#### Scenario: Display SMS message details
- **WHEN** an SMS message is listed
- **THEN** it SHALL show the contact name (or address), the message body, and the readable date

#### Scenario: Display MMS message content
- **WHEN** an MMS message is listed
- **THEN** it SHALL show the body if available, or a placeholder if it contains only media parts (media display is out of scope for initial phase)

### Requirement: Responsive UI
The system SHALL remain responsive during the parsing of large files.

#### Scenario: Progressive loading
- **WHEN** a large XML file is being parsed
- **THEN** the UI SHALL show a progress indicator and populate the message list incrementally as batches are ready
