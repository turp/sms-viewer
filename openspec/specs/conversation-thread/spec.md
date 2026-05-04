# conversation-thread Specification

## Purpose
TBD - created by archiving change group-by-conversation. Update Purpose after archive.
## Requirements
### Requirement: Message Thread Display
The system SHALL display all messages for the selected conversation in chronological order (oldest first) in a right pane.

#### Scenario: Sent messages are right-aligned
- **WHEN** a message in the thread has IsSent equal to true
- **THEN** it SHALL be displayed aligned to the right with a distinct background color

#### Scenario: Received messages are left-aligned
- **WHEN** a message in the thread has IsSent equal to false
- **THEN** it SHALL be displayed aligned to the left with a distinct background color

#### Scenario: Each message shows its body and date
- **WHEN** a message is displayed in the thread
- **THEN** it SHALL show the message body and the human-readable date

#### Scenario: MMS with no text body shows placeholder
- **WHEN** an MMS message has an empty body (media-only)
- **THEN** it SHALL display a placeholder such as "[Media message]"

### Requirement: Message Direction Detection
The system SHALL determine whether each message was sent or received.

#### Scenario: SMS sent detection
- **WHEN** an SMS message has type equal to 2
- **THEN** IsSent SHALL be true

#### Scenario: SMS received detection
- **WHEN** an SMS message has type equal to 1
- **THEN** IsSent SHALL be false

#### Scenario: MMS sent detection
- **WHEN** an MMS message has msg_box equal to 2
- **THEN** IsSent SHALL be true

#### Scenario: MMS received detection
- **WHEN** an MMS message has msg_box equal to 1 or msg_box is absent
- **THEN** IsSent SHALL be false

